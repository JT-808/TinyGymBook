// Models/Satz.cs
using Microsoft.UI.Xaml.Data;
using SQLite;

namespace Tiny_GymBook.Models;

[Bindable]
[Table("Satz")]
public class Satz
{
    [PrimaryKey, AutoIncrement]
    public int Satz_Id { get; set; }

    public int Nummer { get; set; }

    public double Gewicht { get; set; }

    public int Wiederholungen { get; set; }

    public string Kommentar { get; set; } = string.Empty;
    public string Training_Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");


    [Indexed]
    public int Uebung_Id { get; set; }   // FK auf Uebung
}
