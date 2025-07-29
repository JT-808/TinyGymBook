using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Models;

[Bindable]
public class Trainingsplan
{
    public string Name { get; set; } = string.Empty;

    public List<Uebung> Uebungen { get; set; } = new();

    // Parameterloser Konstruktor für JSON-Deserialisierung
    public Trainingsplan() { }

    // Optionaler Komfort-Konstruktor für deinen Code
    public Trainingsplan(string name, List<Uebung> uebungen)
    {
        Name = name;
        Uebungen = uebungen;
    }

    public void AddUebung(Uebung uebung) => Uebungen.Add(uebung);
}
