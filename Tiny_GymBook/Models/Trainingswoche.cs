using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tiny_GymBook.Models;

public class Trainingswoche
{
    public int KalenderWoche { get; }
    public int Jahr { get; }
    public DateTime StartDatum { get; } // Montag
    public DateTime EndDatum { get; }   // Sonntag
    public ObservableCollection<Trainingseintrag> Eintraege { get; } = new ObservableCollection<Trainingseintrag>();

    public Trainingswoche(int kalenderWoche, int jahr, DateTime startDatum)
    {
        KalenderWoche = kalenderWoche;
        Jahr = jahr;
        StartDatum = startDatum;
        EndDatum = startDatum.AddDays(6);
    }

    public void UebernehmeTrainingsplan(Trainingsplan plan)
    {
        foreach (var uebung in plan.Uebungen)
        {
            var eintrag = new Trainingseintrag(uebung);
            AddEintrag(eintrag);
        }
    }

    public void AddEintrag(Trainingseintrag eintrag) => Eintraege.Add(eintrag);

    public ObservableCollection<Uebung> GetUebungen() => new ObservableCollection<Uebung>(Eintraege.Select(e => e.Uebung));
}
