// Models/Satz.cs
using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Models;

[Bindable]
public class Satz
{
    public int Nummer { get; set; }
    public double Gewicht { get; set; }
    public int Wiederholungen { get; set; }
    public string Kommentar { get; set; } = string.Empty;
}
