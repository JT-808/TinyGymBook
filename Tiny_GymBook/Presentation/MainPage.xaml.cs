using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tiny_GymBook.Models;
using System.Collections.ObjectModel;
using System;

namespace Tiny_GymBook.Presentation;

public sealed partial class MainPage : Page
{
    public ObservableCollection<Trainingseintrag> Eintraege { get; }
        = new ObservableCollection<Trainingseintrag>();

    public MainPage()
    {
        this.InitializeComponent(); // Wird automatisch generiert
        LadeBeispielDaten();
        AktualisiereWochenHeader();
    }


    private void LadeBeispielDaten()
    {
        var bankdruecken = new Uebung("Bankdrücken", Muskelgruppe.Brust);
        var kniebeugen = new Uebung("Kniebeugen", Muskelgruppe.Beine);

        Eintraege.Add(new Trainingseintrag(
            bankdruecken,
            saetze: 3,
            wiederholungen: 12,
            gewicht: 80,
            kommentar: "Leichtes Aufwärmen"));

        Eintraege.Add(new Trainingseintrag(
            kniebeugen,
            saetze: 4,
            wiederholungen: 10,
            gewicht: 120,
            kommentar: "Mit Gewichtsgürtel"));
    }

    private void AktualisiereWochenHeader()
    {
        if (WochenHeader != null)
        {
            var kw = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now);
            WochenHeader.Text = $"KW {kw} | {DateTime.Today:dd.MM.yyyy} - {DateTime.Today.AddDays(6):dd.MM.yyyy}";
        }
    }

    private void AddSet_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Trainingseintrag eintrag)
        {
            eintrag.Saetze++;
            var index = Eintraege.IndexOf(eintrag);
            Eintraege[index] = eintrag;
        }
    }
}
