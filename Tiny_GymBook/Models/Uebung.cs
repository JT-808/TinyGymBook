// Models/Uebung.cs
using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Models;

[Bindable]
public class Uebung
{
    public string Name { get; set; } = string.Empty;
    public Muskelgruppe Muskelgruppe { get; set; }

    public Uebung() { }

    public Uebung(string name, Muskelgruppe muskelgruppe)
    {
        Name = name;
        Muskelgruppe = muskelgruppe;
    }

    public Uebung Copy() => new Uebung(Name, Muskelgruppe);
}
