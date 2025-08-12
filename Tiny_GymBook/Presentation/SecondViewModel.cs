using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Uno.Extensions.Navigation;
using Tiny_GymBook.Services.DataService;
using Tiny_GymBook.Services.TrainingsplanIO;
using Uno.Extensions.Storage.Pickers;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class SecondViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IDataService _trainingsplanDBService;
    private readonly ITrainingsplanIOService _trainingsplanIOService;


    [ObservableProperty]
    private ObservableCollection<Trainingsplan> trainingsplaene = new();

    [ObservableProperty]
    private Trainingsplan? selectedPlan;
    // CanExecute-Guard für ExportSelectedPlanAsync


    public SecondViewModel(INavigator navigator, IDataService trainingsplanDBService, ITrainingsplanIOService trainingsplanIOService)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanDBService = trainingsplanDBService ?? throw new ArgumentNullException(nameof(trainingsplanDBService));
        _trainingsplanIOService = trainingsplanIOService ?? throw new ArgumentNullException(nameof(trainingsplanIOService));

        Debug.WriteLine($"[DEBUG] Navigator initialisiert: {_navigator.GetType().Name}");

        _ = LadeTrainingsplaeneAsync();
    }

    private async Task LadeTrainingsplaeneAsync()
    {
        Debug.WriteLine("[DEBUG] Starte LadeTrainingsplaeneAsync()");
        var geladenePlaene = await _trainingsplanDBService.LadeTrainingsplaeneAsync();
        Trainingsplaene.Clear();
        foreach (var plan in geladenePlaene)
        {
            Debug.WriteLine($"[DEBUG] Geladen: {plan.Name} mit {plan.Uebungen.Count} Übungen");
            Debug.WriteLine($"[DEBUG] DB-Plan: {plan.Trainingsplan_Id}, Name = {plan.Name}");

            Trainingsplaene.Add(plan);
        }
    }

    // ****** Buttons ********//

    [RelayCommand]
    private async Task AddPlanAsync()
    {
        // Plan erzeugen und speichern
        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());
        await _trainingsplanDBService.SpeichereTrainingsplanAsync(neuerPlan, new List<Tag>());

        // Pläne neu laden
        await LadeTrainingsplaeneAsync();

        // Jetzt die *aktuelle* Instanz (mit ID!) holen
        var insertedPlan = Trainingsplaene.OrderByDescending(p => p.Trainingsplan_Id).FirstOrDefault();

        if (insertedPlan != null)
        {
            Debug.WriteLine($"[DEBUG] Navigiere direkt zum neuen Plan: {insertedPlan.Name}, ID: {insertedPlan.Trainingsplan_Id}");

            // Jetzt korrekt navigieren
            await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: insertedPlan);
        }
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
    {
        Debug.WriteLine($"[DEBUG] Navigiere zu PlanDetailViewModel mit Plan: {plan?.Name}, ID: {plan?.Trainingsplan_Id}");
        await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: plan);
    }



    [RelayCommand]
    private async Task ExportPlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;
        await _trainingsplanIOService.SpeichereTrainingsplanAsync(plan);
        Debug.WriteLine($"[DEBUG] Exportiert: {plan.Name}");
    }


    //TODO: Geht noch nicht!


    [RelayCommand]
    private async Task ImportFromJsonAsync()
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add(".json");

#if WINDOWS
    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        Stream? stream = null;

        // 1) Versuche echten Dateipfad zu nutzen (Linux/Skia: "file:///.../%20...")
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

        // 2) Fallback: Inhalt über WinRT lesen (funktioniert auch ohne nutzbaren Path)
        if (stream is null)
        {
            var json = await FileIO.ReadTextAsync(file);
            stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        await using (stream)
        {
            var plaene = await _trainingsplanIOService.LadeTrainingsplaeneAsync(stream);
            var list = plaene.ToList();

            Debug.WriteLine($"[IMPORT] Datei enthält {list.Count} Plan/Pläne.");
            if (list.Count == 0)
            {
                Debug.WriteLine("[IMPORT] Parser hat nichts geliefert.");
                return;
            }

            foreach (var p in list)
            {
                // IDs zurücksetzen -> Inserts erzwingen
                p.Trainingsplan_Id = 0;
                foreach (var u in p.Uebungen ?? Enumerable.Empty<Uebung>())
                { u.Uebung_Id = 0; u.Trainingsplan_Id = 0; u.TagId = 0; }

                await _trainingsplanDBService.SpeichereTrainingsplanAsync(p, new List<Tag>());
                Debug.WriteLine($"[IMPORT] Nach Insert: PlanId={p.Trainingsplan_Id}");

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
    {
        await _navigator.NavigateBackAsync(this);
    }
}
