using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Presentation;

public partial class MainViewModel : ObservableObject
{

    [ObservableProperty]
    private string wochenHeaderText;
    private readonly INavigator _navigator;

    // ObservableCollection von Trainingseinträgen
    public ObservableCollection<Trainingseintrag> Eintraege { get; } = new();

    public MainViewModel(INavigator navigator)
    {
        LadeBeispielDaten();

        //Kalenderwoche
        var kw = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now);
        WochenHeaderText = $"KW {kw} | {DateTime.Today:dd.MM.yyyy} - {DateTime.Today.AddDays(6):dd.MM.yyyy}";

        _navigator = navigator;

    }

    [RelayCommand]
    private async Task NavigateToPlaeneAsync()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }

    private void LadeBeispielDaten()
    {
        var bankdruecken = new Uebung("Bankdrücken", Muskelgruppe.Brust);
        var kniebeugen = new Uebung("Kniebeugen", Muskelgruppe.Beine);

        // Verwende Konstruktor mit Satz-Parametern, damit Saetze direkt gesetzt werden
        Eintraege.Add(new Trainingseintrag(bankdruecken, 3, 12, 80, "Leichtes Aufwärmen"));
        Eintraege.Add(new Trainingseintrag(kniebeugen, 4, 10, 120, "Mit Gewichtsgürtel"));
    }

    [RelayCommand]
    public void AddSatz(Trainingseintrag eintrag)
    {
        if (eintrag == null) return;

        int neueNummer = eintrag.Saetze.Count + 1;

        var neuerSatz = new Satz
        {
            Nummer = neueNummer,
            Gewicht = 0,
            Wiederholungen = 0,
            Kommentar = string.Empty
        };

        eintrag.Saetze.Add(neuerSatz);
    }
}
