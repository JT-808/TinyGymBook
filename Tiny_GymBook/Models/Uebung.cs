// Models/Uebung.cs
using Microsoft.UI.Xaml.Data;
using SQLite;

namespace Tiny_GymBook.Models;

[Bindable]
public class Uebung
{
    [PrimaryKey, AutoIncrement]
    public int Uebung_Id { get; set; }
    [NotNull]
    public string Name { get; set; } = string.Empty;
    [Indexed]
    public Muskelgruppe Muskelgruppe { get; set; }

    public int Trainingsplan_Id { get; set; }
    public int TagId { get; set; }

    [Ignore] // Damit SQLite das Feld ignoriert
    public Trainingseintrag? Trainingseintrag { get; set; }

    public Uebung() { }

    public Uebung(string name, Muskelgruppe muskelgruppe)
    {
        Name = name;
        Muskelgruppe = muskelgruppe;
    }

    public Uebung Copy() => new Uebung(Name, Muskelgruppe);
}
