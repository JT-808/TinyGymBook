// Models/Trainingswoche.cs
using System;
using System.Collections.Generic;

namespace Tiny_GymBook.Models;

public class Trainingswoche
{
    public int KalenderWoche { get; }
    public int Jahr { get; }
    public DateTime StartDatum { get; } // Montag
    public DateTime EndDatum { get; }   // Sonntag
    public List<Trainingseintrag> Eintraege { get; } = new List<Trainingseintrag>();

    public Trainingswoche(int kalenderWoche, int jahr, DateTime startDatum)
    {
        KalenderWoche = kalenderWoche;
        Jahr = jahr;
        StartDatum = startDatum;
        EndDatum = startDatum.AddDays(6); // +6 Tage = Sonntag
    }

    public void UebernehmeTrainingsplan(Trainingsplan plan)
    {
        // Implementierung folgt
    }

    public void AddEintrag(Trainingseintrag eintrag) => Eintraege.Add(eintrag);

    public List<Uebung> GetUebungen() => Eintraege.ConvertAll(e => e.Uebung);
}
