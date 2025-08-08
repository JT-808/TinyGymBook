using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;
using SQLite;

namespace Tiny_GymBook.Models;

[Bindable]
public class Trainingsplan
{
    [PrimaryKey, AutoIncrement]
    public int Trainingsplan_Id { get; set; }
    [NotNull]
    public string Name { get; set; } = string.Empty;

    //Collection Uebungen muss weg, da Verweis auf tage genutzt werden soll

    [Ignore]
    public ObservableCollection<Uebung> Uebungen { get; set; } = new();

    [Ignore]
    public ObservableCollection<Tag> tage { get; set; } = new();

    public Trainingsplan() { }

    public Trainingsplan(string name, IEnumerable<Uebung> uebungen)
    {
        Name = name;
        Uebungen = new ObservableCollection<Uebung>(uebungen);
    }
}
