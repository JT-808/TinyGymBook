using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Tiny_GymBook.Models;
using Uno.Extensions.Navigation;
using Tiny_GymBook.Services.DataService;
using Tiny_GymBook.Services.TrainingsplanIO;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class SecondViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IDataService _trainingsplanDBService;
    private readonly ITrainingsplanIOService _trainingsplanIOService;

    [ObservableProperty] private ObservableCollection<Trainingsplan> trainingsplaene = new();
    [ObservableProperty] private Trainingsplan? selectedPlan;

    // gleiche JSON-Optionen wie im Service
    private static readonly JsonSerializerOptions _jsonOptionen = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SecondViewModel(
        INavigator navigator,
        IDataService trainingsplanDBService,
        ITrainingsplanIOService trainingsplanIOService)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanDBService = trainingsplanDBService ?? throw new ArgumentNullException(nameof(trainingsplanDBService));
        _trainingsplanIOService = trainingsplanIOService ?? throw new ArgumentNullException(nameof(trainingsplanIOService));

        _ = LadeTrainingsplaeneAsync();
    }

    private async Task LadeTrainingsplaeneAsync()
    {
        var geladenePlaene = await _trainingsplanDBService.LadeTrainingsplaeneAsync();
        Trainingsplaene.Clear();
        foreach (var plan in geladenePlaene)
            Trainingsplaene.Add(plan);
    }



    //===================BUTTONS==================


    [RelayCommand]
    private async Task AddPlanAsync()
    {
        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());
        await _trainingsplanDBService.SpeichereTrainingsplanAsync(neuerPlan, new List<Tag>());
        await LadeTrainingsplaeneAsync();

        var inserted = Trainingsplaene.OrderByDescending(p => p.Trainingsplan_Id).FirstOrDefault();
        if (inserted is not null)
            await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: inserted);
    }

    [RelayCommand]
    private async Task DeletePlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;
        await _trainingsplanDBService.LoescheTrainingsplanAsync(plan);
        await LadeTrainingsplaeneAsync();
    }

    [RelayCommand]
    private async Task OpenPlanAsync(Trainingsplan plan)
        => await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: plan);

    // ===== Export über FolderPicker (Linux & Android) =====
    [RelayCommand]
    private async Task ExportPlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;

        try
        {
            // 1) Ordner wählen
            var folderPicker = new FolderPicker();
            // Auf Linux/Android ist keine Fenster-Initialisierung nötig.
            // (Für Windows wäre InitializeWithWindow erforderlich.)

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is null)
            {
                Debug.WriteLine("[EXPORT] Abgebrochen (kein Ordner gewählt).");
                return;
            }

            // 2) Datei im gewählten Ordner erzeugen
            var fileName = $"{Sanitize(plan.Name)}.json";
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            // 3) Plan serialisieren & schreiben
            var json = JsonSerializer.Serialize(plan, _jsonOptionen);
            await FileIO.WriteTextAsync(file, json);

            Debug.WriteLine($"[EXPORT] Gespeichert: {file.Path}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EXPORT] Fehler: {ex.Message}");
        }
    }


    [RelayCommand]
    private async Task ImportFromJsonAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        Stream? stream = null;

        var path = file.Path;
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    path = new Uri(path).LocalPath;
                path = Uri.UnescapeDataString(path);
                stream = System.IO.File.OpenRead(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IMPORT] File.OpenRead-Fallback nötig: {ex.Message}");
            }
        }

        if (stream is null)
        {
            var json = await FileIO.ReadTextAsync(file);
            stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        await using (stream)
        {
            var plaene = (await _trainingsplanIOService.LadeTrainingsplaeneAsync(stream)).ToList();
            if (plaene.Count == 0) return;

            foreach (var p in plaene)
            {
                p.Trainingsplan_Id = 0;
                foreach (var u in p.Uebungen ?? Enumerable.Empty<Uebung>())
                {
                    u.Uebung_Id = 0; u.Trainingsplan_Id = 0; u.TagId = 0;
                }

                await _trainingsplanDBService.SpeichereTrainingsplanAsync(p, new List<Tag>());

                var tage = await _trainingsplanDBService.LadeTageAsync(p.Trainingsplan_Id);
                var ersterTag = tage.OrderBy(t => t.Reihenfolge).FirstOrDefault();
                if (ersterTag is null) continue;

                foreach (var u in p.Uebungen ?? Enumerable.Empty<Uebung>())
                {
                    u.Trainingsplan_Id = p.Trainingsplan_Id;
                    u.TagId = ersterTag.TagId;
                    await _trainingsplanDBService.SpeichereUebung(u);
                }
            }
        }

        await LadeTrainingsplaeneAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
        => await _navigator.NavigateBackAsync(this);

    // ===== Helpers =====
    private static string Sanitize(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim('_');
    }
}
