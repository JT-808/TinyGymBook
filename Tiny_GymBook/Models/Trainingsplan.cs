// Models/Trainingsplan.cs
using System.Collections.Generic;

namespace Tiny_GymBook.Models;

public class Trainingsplan
{
    public string Name { get; set; }
    public List<Uebung> Uebungen { get; set; } = new List<Uebung>();

    public Trainingsplan(string name, List<Uebung> uebungen)
    {
        Name = name;
        Uebungen = uebungen;
    }

    public void AddUebung(Uebung uebung) => Uebungen.Add(uebung);
}
