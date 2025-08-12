using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Tiny_GymBook.Services.DataService;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IDataService _trainingsplanDBService;

    [ObservableProperty]
    private Trainingswoche? aktuelleWoche;

    [ObservableProperty]
    private Trainingsplan? aktiverPlan;

    public ObservableCollection<Trainingsplan> AllePlaene { get; } = new();
    public ObservableCollection<Tag> AlleTage { get; } = new(); // HIER: Alle Tage + Übungen + Sätze!

    public MainViewModel(INavigator navigator, IDataService trainingsplanService)
    {
        _navigator = navigator;
        _trainingsplanDBService = trainingsplanService;
        InitializeWeek();
        _ = LadeAllePlaeneAsync();
    }

    private async Task LadeAllePlaeneAsync()
    {
        var plaene = await _trainingsplanDBService.LadeTrainingsplaeneAsync();
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

    // Immer wenn sich der aktive Plan ändert, lade die Tage/Übungen/Sätze neu!
    partial void OnAktiverPlanChanged(Trainingsplan? value)
    {
        _ = LadeTageMitUebungenUndEintraegenAsync();
    }

    private async Task LadeTageMitUebungenUndEintraegenAsync()
    {
        if (AktiverPlan == null)
        {
            AlleTage.Clear();
            return;
        }

        var tage = await _trainingsplanDBService.LadeTageAsync(AktiverPlan.Trainingsplan_Id);
        var uebungen = await _trainingsplanDBService.LadeUebungenZuPlanAsync(AktiverPlan.Trainingsplan_Id);

        AlleTage.Clear();

        foreach (var tag in tage)
        {
            tag.Uebungen.Clear();

            var uebungenFuerTag = uebungen.Where(u => u.TagId == tag.TagId);

            foreach (var uebung in uebungenFuerTag)
            {
                // Sätze direkt für die Übung laden (Optional: Wochen-/Datumsfilter hinzufügen)
                var saetze = await _trainingsplanDBService.LadeSaetzeFuerUebungAsync(uebung.Uebung_Id /*, filter*/);
                uebung.Saetze = new ObservableCollection<Satz>(saetze);

                tag.Uebungen.Add(uebung);
            }

            AlleTage.Add(tag);
        }
    }

    [RelayCommand]
    public void AddSatz(Uebung uebung)
    {
        if (uebung == null)
            return;

        int neueNummer = uebung.Saetze.Count + 1;
        uebung.Saetze.Add(new Satz
        {
            Nummer = neueNummer,
            Uebung_Id = uebung.Uebung_Id,  // <-- Stelle sicher, dass Satz das FK-Feld hat!
            Gewicht = 0,
            Wiederholungen = 0,
            Kommentar = string.Empty,
            Training_Date = DateTime.Today.ToString("yyyy-MM-dd") // falls im Satz vorhanden
        });
    }

    [RelayCommand]
    private async Task NavigateToPlaeneAsync()
    {
        // Speichere alle Übungen + deren Sätze
        foreach (var tag in AlleTage)
        {
            foreach (var uebung in tag.Uebungen)
            {
                await _trainingsplanDBService.SpeichereUebung(uebung);
                await _trainingsplanDBService.SpeichereSaetzeFuerUebungAsync(uebung.Uebung_Id, uebung.Saetze);
            }
        }

        await LadeTageMitUebungenUndEintraegenAsync();
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }
}
