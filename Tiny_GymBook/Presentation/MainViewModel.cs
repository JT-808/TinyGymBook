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
    private readonly INavigator _navigator;
    private readonly ITrainingsplanService _trainingsplanService;

    [ObservableProperty]
    private Trainingswoche? aktuelleWoche;

    [ObservableProperty]
    private Trainingsplan? aktiverPlan;

    public ObservableCollection<Trainingsplan> AllePlaene { get; } = new();
    public ObservableCollection<Tag> AlleTage { get; } = new(); // HIER: Alle Tage + Übungen + Sätze!

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

        // 1. Tage und Übungen zum Plan laden
        var tage = await _trainingsplanService.LadeTageAsync(AktiverPlan.Trainingsplan_Id);
        var uebungen = await _trainingsplanService.LadeUebungenZuPlanAsync(AktiverPlan.Trainingsplan_Id);

        // 2. Trainingseinträge laden (z.B. für aktuelle Woche, oder alles)
        var eintraege = await _trainingsplanService.LadeAlleTrainingseintraegeAsync();

        AlleTage.Clear();

        foreach (var tag in tage)
        {
            tag.Uebungen.Clear();

            // Alle Übungen für diesen Tag
            var uebungenFuerTag = uebungen.Where(u => u.TagId == tag.TagId);

            foreach (var uebung in uebungenFuerTag)
            {
                // Finde passenden Eintrag
                var eintrag = eintraege.FirstOrDefault(e =>
                    e.Trainingsplan_Id == AktiverPlan.Trainingsplan_Id &&
                    e.Uebung_Id == uebung.Uebung_Id
                // Hier könnte man noch nach Datum/Woche filtern!
                );

                // Falls noch kein Eintrag für diese Übung existiert, erstelle einen leeren
                if (eintrag == null)
                {
                    eintrag = new Trainingseintrag
                    {
                        Trainingsplan_Id = AktiverPlan.Trainingsplan_Id,
                        Uebung_Id = uebung.Uebung_Id,
                        Uebung = uebung,
                        Saetze = new ObservableCollection<Satz>()
                    };
                }
                else
                {
                    // Sätze laden
                    var saetze = await _trainingsplanService.LadeSaetzeFuerEintragAsync(eintrag.Eintrag_Id);
                    eintrag.Saetze = new ObservableCollection<Satz>(saetze);
                }

                uebung.Trainingseintrag = eintrag;
                tag.Uebungen.Add(uebung);
            }
            AlleTage.Add(tag);
        }
    }

    [RelayCommand]
    public void AddSatz(Uebung uebung)
    {
        if (uebung == null || uebung.Trainingseintrag == null)
            return;

        int neueNummer = (uebung.Trainingseintrag.Saetze.Count) + 1;
        uebung.Trainingseintrag.Saetze.Add(new Satz
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
        // Speichere alle Trainingseinträge mit ihren Sätzen
        foreach (var tag in AlleTage)
        {
            foreach (var uebung in tag.Uebungen)
            {
                if (uebung.Trainingseintrag != null)
                    await _trainingsplanService.SpeichereTrainingseintragAsync(uebung.Trainingseintrag);
            }
        }
        await LadeTageMitUebungenUndEintraegenAsync();
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }
}
