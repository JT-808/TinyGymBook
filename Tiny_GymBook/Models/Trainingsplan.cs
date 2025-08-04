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

    [Ignore]
    public ObservableCollection<Uebung> Uebungen { get; set; } = new();

    public Trainingsplan() { }

    public Trainingsplan(string name, IEnumerable<Uebung> uebungen)
    {
        Name = name;
        Uebungen = new ObservableCollection<Uebung>(uebungen);
    }

    public void AddUebung(Uebung uebung) => Uebungen.Add(uebung);
}
