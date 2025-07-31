using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class MainViewModel : ObservableObject
{

    [ObservableProperty]
    private INavigator _navigator;

    [ObservableProperty]
    private Trainingswoche? aktuelleWoche;

    // ObservableCollection von Trainingseinträgen
    public ObservableCollection<Trainingseintrag> Eintraege { get; } = new();

    public MainViewModel(INavigator navigator)
    {
        _navigator = navigator;
        InitializeWeek();
        LadeBeispielDaten();

    }

    private void InitializeWeek()
    {
        var heute = DateTime.Today;
        var kw = System.Globalization.ISOWeek.GetWeekOfYear(heute);
        var montag = heute.AddDays(-(int)heute.DayOfWeek + (int)DayOfWeek.Monday);
        AktuelleWoche = new Trainingswoche(kw, heute.Year, montag);
    }

    [RelayCommand]
    private async Task NavigateToPlaeneAsync()
    {
        await Navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }

    private void LadeBeispielDaten()
    {
        var bankdruecken = new Uebung("Bankdrücken", Muskelgruppe.Brust);
        var kniebeugen = new Uebung("Kniebeugen", Muskelgruppe.Beine);

        // Verwende Konstruktor mit Satz-Parametern, damit Saetze direkt gesetzt werden
        Eintraege.Add(new Trainingseintrag(bankdruecken, 1, 3, 12, 80, "Leichtes Aufwärmen", "2025-09-01"));
        Eintraege.Add(new Trainingseintrag(kniebeugen, 1, 4, 10, 120, "Mit Gewichtsgürtel", "2025-09-01"));
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
