using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Tiny_GymBook.Services.Trainingsplanservice;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;
    private readonly ITrainingsplanService _trainingsplanService;

    [ObservableProperty]
    private Trainingswoche? aktuelleWoche;

    [ObservableProperty]
    private Trainingsplan? aktiverPlan;
    public ObservableCollection<Trainingsplan> AllePlaene { get; } = new();
    public ObservableCollection<Trainingseintrag> Eintraege { get; } = new();

    public MainViewModel(INavigator navigator, ITrainingsplanService trainingsplanService)
    {
        _navigator = navigator;
        _trainingsplanService = trainingsplanService;
        InitializeWeek();
        _ = LadeAllePlaeneAsync();
    }

    private async Task LadeAllePlaeneAsync()
    {
        var plaene = await _trainingsplanService.LadeTrainingsplaeneAsync();
        AllePlaene.Clear();
        foreach (var plan in plaene)
            AllePlaene.Add(plan);

        if (AktiverPlan == null && AllePlaene.Any())
            AktiverPlan = AllePlaene.First();
    }

    private void InitializeWeek()
    {
        var heute = DateTime.Today;
        var kw = System.Globalization.ISOWeek.GetWeekOfYear(heute);
        var montag = heute.AddDays(-(int)heute.DayOfWeek + (int)DayOfWeek.Monday);
        AktuelleWoche = new Trainingswoche(kw, heute.Year, montag);
    }
    // Wenn andere Plan ausgewäht wird -> Einträge aktualisieren
    partial void OnAktiverPlanChanged(Trainingsplan? value)
    {
        _ = LadeEintraegeAsync();
    }


    private async Task LadeEintraegeAsync()
    {
        if (AktiverPlan == null)
        {
            Eintraege.Clear();
            return;
        }

        // Lade alle Einträge aus der DB (du kannst auch nach Plan filtern, falls du willst)
        var eintraegeAusDB = await _trainingsplanService.LadeAlleTrainingseintraegeAsync();

        Eintraege.Clear();

        foreach (var eintrag in eintraegeAusDB)
        {
            // Lade die zugehörigen Sätze für diesen Eintrag
            var saetze = await _trainingsplanService.LadeSaetzeFuerEintragAsync(eintrag.Eintrag_Id);
            eintrag.Saetze = new ObservableCollection<Satz>(saetze);

            Eintraege.Add(eintrag);

            Debug.WriteLine($"[DEBUG] Eintrag {eintrag.Eintrag_Id} geladen mit {eintrag.Saetze.Count} Sätzen.");
        }

        // Optional: Wenn keine Einträge da sind, einen leeren Eintrag für aktuellen Plan hinzufügen:
        if (Eintraege.Count == 0)
            Eintraege.Add(new Trainingseintrag { Trainingsplan_Id = AktiverPlan.Trainingsplan_Id });
    }


    [RelayCommand]
    public void AddSatz(Trainingseintrag eintrag)
    {
        if (eintrag == null)
            return;

        int neueNummer = eintrag.Saetze.Count + 1;

        eintrag.Saetze.Add(new Satz
        {
            Nummer = neueNummer,
            Gewicht = 0,
            Wiederholungen = 0,
            Kommentar = string.Empty
        });
    }


    [RelayCommand]
    private async Task NavigateToPlaeneAsync()
    {
        foreach (var eintrag in Eintraege)
        {
            eintrag.Trainingsplan_Id = AktiverPlan?.Trainingsplan_Id ?? eintrag.Trainingsplan_Id;
            await _trainingsplanService.SpeichereTrainingseintragAsync(eintrag);
        }
        await LadeEintraegeAsync();
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }










    // eventuell für später um einzelne Extra-Übungen hinzufügen zu können

    // [RelayCommand]
    // public void AddTrainingseintrag()
    // {
    //     if (AktiverPlan == null) return;

    //     var neuerEintrag = new Trainingseintrag
    //     {
    //         Trainingsplan_Id = AktiverPlan.Trainingsplan_Id,
    //         Eintrag_Id = 0,
    //         Kommentar = $"Neuer Eintrag {DateTime.Now:T}",
    //         Training_Date = DateTime.Now.ToString("yyyy-MM-dd"),
    //         Uebung = new Uebung("Neue Übung", Muskelgruppe.Brust)
    //     };

    //     neuerEintrag.Saetze.Add(new Satz
    //     {
    //         Nummer = 1,
    //         Gewicht = 0,
    //         Wiederholungen = 0,
    //         Kommentar = string.Empty
    //     });

    //     Eintraege.Add(neuerEintrag);
    // }
}
