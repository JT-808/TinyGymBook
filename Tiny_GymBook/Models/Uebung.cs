// Models/Uebung.cs
namespace Tiny_GymBook.Models;

public class Uebung
{
    public string Name { get; set; }
    public Muskelgruppe Muskelgruppe { get; set; }

    public Uebung(string name, Muskelgruppe muskelgruppe)
    {
        Name = name;
        Muskelgruppe = muskelgruppe;
    }

    public Uebung Copy() => new Uebung(Name, Muskelgruppe);
}
