using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Presentation;

public partial class SecondViewModel : ObservableObject
{
    // Collection für ListView
    [ObservableProperty]
    private ObservableCollection<Trainingsplan> trainingsplaene = new();

    // Optional: Ausgewählter Plan (nicht zwingend notwendig für Navigation)
    [ObservableProperty]
    private Trainingsplan? selectedPlan;

    public SecondViewModel()
    {
        // Beispiel-Daten (kann später durch Datenbank ersetzt werden)
        Trainingsplaene = new ObservableCollection<Trainingsplan>
        {
            new Trainingsplan("3er Split", new List<Uebung>()),
            new Trainingsplan("4er Split", new List<Uebung>()),
            new Trainingsplan("5er Split", new List<Uebung>()),
            new Trainingsplan("Ganzkörper", new List<Uebung>())
        };
    }

    // Navigation zu Detailansicht (noch zu implementieren)
    [RelayCommand]
    private void OpenPlan(Trainingsplan plan)
    {
        // Hier später Navigation einfügen, z. B.:
        // await Navigator.NavigateViewModelAsync<PlanDetailViewModel, Trainingsplan>(plan);
        System.Diagnostics.Debug.WriteLine($"Navigiere zu Plan: {plan.Name}");
    }

    [RelayCommand]
    private void AddPlan()
    {
        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());
        Trainingsplaene.Add(neuerPlan);
    }
}
