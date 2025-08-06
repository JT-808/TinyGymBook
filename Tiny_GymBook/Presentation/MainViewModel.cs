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
        Debug.WriteLine("[DEBUG] MainViewModel erzeugt!");


        _navigator = navigator;
        _trainingsplanService = trainingsplanService;
        InitializeWeek();
        //LadeBeispielDaten();

        _ = LadeAllePlaeneAsync();

    }


    private void LadeBeispielDaten()
    {
        var bankdruecken = new Uebung("Bankdrücken", Muskelgruppe.Brust);
        var kniebeugen = new Uebung("Kniebeugen", Muskelgruppe.Beine);

        // Verwende Konstruktor mit Satz-Parametern, damit Saetze direkt gesetzt werden
        Eintraege.Add(new Trainingseintrag(bankdruecken, 1, 3, 12, 80, "Leichtes Aufwärmen", "2025-09-01"));
        Eintraege.Add(new Trainingseintrag(kniebeugen, 1, 4, 10, 120, "Mit Gewichtsgürtel", "2025-09-01"));
    }


    private async Task LadeAllePlaeneAsync()
    {
        var plaene = await _trainingsplanService.LadeTrainingsplaeneAsync();
        Debug.WriteLine($"[DEBUG] LadeAllePlaeneAsync() aufgerufen");
        AllePlaene.Clear();
        foreach (var plan in plaene)
        {
            AllePlaene.Add(plan);
        }

        // Wähle einen Standardplan aus
        if (AktiverPlan == null && AllePlaene.Any())
            AktiverPlan = AllePlaene.First();


        if (AllePlaene.Any())
        {
            var testPlan = AllePlaene.First();

            var eintraegeFuerPlan = await _trainingsplanService.LadeAlleTrainingseintraegeAsync();

            if (!eintraegeFuerPlan.Any())
            {
                // Nur wenn noch keine Einträge da sind
                var testUebung = new Uebung("Testübung", Muskelgruppe.Brust);
                var testEintrag = new Trainingseintrag(testUebung, 1, 3, 10, 60, "Test-Kommentar", DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    Trainingsplan_Id = testPlan.Trainingsplan_Id
                };
                await _trainingsplanService.SpeichereTrainingseintragAsync(testEintrag);
                await LadeEintraegeAsync();
            }
        }
    }





    private async Task LadeEintraegeAsync()
    {

        // Test: Lade ALLE Einträge aus DB, Debug-Ausgabe
        var alleEintraege = await _trainingsplanService.LadeAlleTrainingseintraegeAsync();
        foreach (var eintrag in alleEintraege)
        {
            Debug.WriteLine($"[TEST] Eintrag: Id={eintrag.Eintrag_Id}, Trainingsplan_Id={eintrag.Trainingsplan_Id}, TagId={eintrag.TagId}, Kommentar={eintrag.Kommentar}");
        }


        Debug.WriteLine("[DEBUG] Eintraege werden geladen");

        if (AktiverPlan == null)
        {
            Eintraege.Clear();
            return;
        }

        var eintraegeAusDB = await _trainingsplanService.LadeAlleTrainingseintraegeAsync();

        Eintraege.Clear();
        foreach (var eintrag in eintraegeAusDB)
            Eintraege.Add(eintrag);

        // Wenn keine Einträge geladen wurden, füge einen leeren Eintrag für den aktuellen Plan hinzu:
        if (Eintraege.Count == 0 && AktiverPlan != null)
            Eintraege.Add(new Trainingseintrag { Trainingsplan_Id = AktiverPlan.Trainingsplan_Id });
    }

    private void InitializeWeek()
    {
        var heute = DateTime.Today;
        var kw = System.Globalization.ISOWeek.GetWeekOfYear(heute);
        var montag = heute.AddDays(-(int)heute.DayOfWeek + (int)DayOfWeek.Monday);
        AktuelleWoche = new Trainingswoche(kw, heute.Year, montag);
    }
    // wenn anderer Plan gewählt, lade dann neie Einträge
    partial void OnAktiverPlanChanged(Trainingsplan? value)
    {
        // Bei Planwechsel Einträge neu laden!
        _ = LadeEintraegeAsync();
    }

    // ****** Buttons ********//

    [RelayCommand]
    private async Task NavigateToPlaeneAsync()
    {
        Debug.WriteLine($"[DEBUG] AktiverPlan: {AktiverPlan}");
        Debug.WriteLine($"[DEBUG] _trainingsplanService: {_trainingsplanService}");
        Debug.WriteLine($"[DEBUG] _navigator: {_navigator}");
        Debug.WriteLine($"[DEBUG] Eintraege: {Eintraege?.Count}");

        foreach (var eintrag in Eintraege)
        {
            if (AktiverPlan == null)

                eintrag.Trainingsplan_Id = AktiverPlan?.Trainingsplan_Id ?? eintrag.Trainingsplan_Id;

            if (_trainingsplanService == null)

                await _trainingsplanService.SpeichereTrainingseintragAsync(eintrag);
        }

        await LadeEintraegeAsync();

        // if (_navigator == null)

        await _navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }

    // [RelayCommand]
    // public void AddSatz(Trainingseintrag eintrag)
    // {
    //     if (eintrag == null) return;

    //     int neueNummer = eintrag.Saetze.Count + 1;

    //     var neuerSatz = new Satz
    //     {
    //         Nummer = neueNummer,
    //         Gewicht = 0,
    //         Wiederholungen = 0,
    //         Kommentar = string.Empty
    //     };

    //     eintrag.Saetze.Add(neuerSatz);
    // }

    [RelayCommand]
    public void AddTrainingseintrag()
    {
        if (AktiverPlan == null) return;

        var neuerEintrag = new Trainingseintrag
        {
            Trainingsplan_Id = AktiverPlan.Trainingsplan_Id,
            Eintrag_Id = 0,
            Kommentar = $"Neuer Eintrag {DateTime.Now:T}",
            Training_Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Uebung = new Uebung("Neue Übung", Muskelgruppe.Brust) // oder dynamisch wählen!
        };

        // erstelle den ersten Satz  automatisch

        neuerEintrag.Saetze.Add(new Satz
        {
            Nummer = 1,
            Gewicht = 0,
            Wiederholungen = 0,
            Kommentar = string.Empty
        });

        Eintraege.Add(neuerEintrag);

    }
}
