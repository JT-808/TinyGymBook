using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Presentation;

public partial class PlanDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Trainingsplan trainingsplan;

    [ObservableProperty]
    private string aktuellerTag = "";

    public ObservableCollection<Uebung> Uebungen { get; } = new();

    public PlanDetailViewModel()
    {
        // DEMO-Daten, später mit echten Daten ersetzen!
        trainingsplan = new Trainingsplan("5er Split", new List<Uebung>
        {
            new Uebung { Name = "Bankdrücken" },
            new Uebung { Name = "Schrägbankdrücken" }
        });

        aktuellerTag = "Tag 1";

        foreach (var uebung in trainingsplan.Uebungen)
        {
            Uebungen.Add(uebung);
        }
    }

    [RelayCommand]
    private void AddUebung()
    {
        Uebungen.Add(new Uebung { Name = "Neue Übung" });
    }

    [RelayCommand]
    private void AddTag()
    {
        // Logik zum Hinzufügen eines Tages
    }
}
