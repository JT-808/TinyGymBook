using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Models;

[Bindable]
public class Trainingseintrag
{
    public Uebung Uebung { get; set; }
    public ObservableCollection<Satz> Saetze { get; set; } = new();


    public Trainingseintrag(Uebung uebung)
    {
        Uebung = uebung.Copy();

        // Beispiel: Standard 3 Sätze, 10 Wiederholungen, 0 Gewicht, leerer Kommentar
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
    }
}
