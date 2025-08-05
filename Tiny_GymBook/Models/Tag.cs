using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;
using SQLite;

namespace Tiny_GymBook.Models;

[Bindable]
public class Tag
{
    [PrimaryKey, AutoIncrement]
    public int TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Reihenfolge { get; set; }
    public int Trainingsplan_Id { get; set; }

    [Ignore]
    public ObservableCollection<Uebung> Uebungen { get; } = new();
}

