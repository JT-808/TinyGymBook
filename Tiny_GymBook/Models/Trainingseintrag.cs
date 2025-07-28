// Models/Trainingseintrag.cs
namespace Tiny_GymBook.Models;

public class Trainingseintrag
{
    public Uebung Uebung { get; set; }
    public int Saetze { get; set; }
    public int Wiederholungen { get; set; }
    public double Gewicht { get; set; }
    public string Kommentar { get; set; }

    public Trainingseintrag(Uebung uebung, int saetze, int wiederholungen, double gewicht, string kommentar)
    {
        Uebung = uebung.Copy();
        Saetze = saetze;
        Wiederholungen = wiederholungen;
        Gewicht = gewicht;
        Kommentar = kommentar;
    }
}
