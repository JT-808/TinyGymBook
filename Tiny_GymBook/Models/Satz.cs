// Models/Satz.cs
namespace Tiny_GymBook.Models;

public class Satz
{
    public int Nummer { get; set; }
    public double Gewicht { get; set; }
    public int Wiederholungen { get; set; }
    public string Kommentar { get; set; } = string.Empty;
}
