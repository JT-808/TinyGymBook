using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;
using SQLite;

namespace Tiny_GymBook.Models;

[Bindable]
public class Trainingseintrag
{
    [PrimaryKey, AutoIncrement]
    public int Eintrag_Id { get; set; }

    [Indexed]
    public int Uebung_Id { get; set; }

    [Indexed]
    public int Trainingsplan_Id { get; set; }

    public string Kommentar { get; set; } = string.Empty;

    [Ignore]
    public Uebung? Uebung { get; set; }

    [Ignore]
    public ObservableCollection<Satz> Saetze { get; set; } = new();

    // Nur zur Initialisierung – nicht speichern
    [Ignore]
    public int StandardWiederholungen { get; set; }

    [Ignore]
    public double StandardGewicht { get; set; }

    public Trainingseintrag() { }

    public Trainingseintrag(Uebung uebung)
    {
        Uebung = uebung.Copy();
        Uebung_Id = uebung.Uebung_Id;

        for (int i = 1; i <= 3; i++)
        {
            Saetze.Add(new Satz
            {
                Nummer = i,
                Gewicht = 0,
                Wiederholungen = 10,
                Kommentar = string.Empty
            });
        }
    }

    public Trainingseintrag(Uebung uebung, int anzahlSaetze, int wiederholungen, double gewicht, string kommentar)
    {
        Uebung = uebung.Copy();
        Uebung_Id = uebung.Uebung_Id;
        Kommentar = kommentar;

        for (int i = 1; i <= anzahlSaetze; i++)
        {
            Saetze.Add(new Satz
            {
                Nummer = i,
                Gewicht = gewicht,
                Wiederholungen = wiederholungen,
                Kommentar = kommentar
            });
        }

        StandardWiederholungen = wiederholungen;
        StandardGewicht = gewicht;
    }
}
