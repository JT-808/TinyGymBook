using System.Collections.ObjectModel;
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

    public string Kommentar { get; set; } = string.Empty;

    public string Training_Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");


    public int Trainingsplan_Id { get; set; }

    [Indexed]
    public int TagId { get; set; }


    [Ignore]
    public ObservableCollection<Satz> Saetze { get; set; } = new();

    public Uebung() { }

    public Uebung(string name, Muskelgruppe muskelgruppe)
    {
        Saetze = new ObservableCollection<Satz>();
        Name = name;
        Kommentar = string.Empty;
        Muskelgruppe = muskelgruppe;
    }

    public static Uebung CreateNew(int trainingsplanId, int tagId, string? name = null, Muskelgruppe muskel = default)
    {
        return new Uebung
        {
            Name = name ?? "Neue Übung",
            Muskelgruppe = muskel, // Achtung: default(Enum)=0 -> stell sicher, dass 0 ein gültiger Wert ist
            Trainingsplan_Id = trainingsplanId,
            TagId = tagId,
            Kommentar = string.Empty,
            Training_Date = DateTime.Today.ToString("yyyy-MM-dd"),
            Saetze = new ObservableCollection<Satz>()
        };
    }
}
