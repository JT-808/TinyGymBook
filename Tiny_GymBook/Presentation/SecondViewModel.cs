using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Presentation;

public partial class SecondViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private ObservableCollection<Trainingsplan> trainingsplaene;

    [ObservableProperty]
    private Trainingsplan? selectedPlan;

    public SecondViewModel(INavigator navigator)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        Trainingsplaene = new(); // <- direkte Initialisierung
        InitializeData();
        Debug.WriteLine($"Navigator initialisiert: {_navigator.GetType().Name}");
    }

    private void InitializeData()
    {
        Trainingsplaene.Clear();
        Trainingsplaene.Add(new("3er Split", new List<Uebung>
    {
        new("Bankdrücken", Muskelgruppe.Brust),
        new("Kreuzheben", Muskelgruppe.Rücken),
        new("Kniebeuge", Muskelgruppe.Beine)
    }));
        Trainingsplaene.Add(new("4er Split", new List<Uebung>
    {
        new("Military Press", Muskelgruppe.Schultern),
        new("Latzug", Muskelgruppe.Rücken),
        new("Beinpresse", Muskelgruppe.Beine),
        new("Bizepscurls", Muskelgruppe.Arme)
    }));
    }

    [RelayCommand]
    private async Task OpenPlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;

        Debug.WriteLine($"Öffne Plan: {plan.Name}");
        await Task.CompletedTask; // Platzhalter für zukünftige Navigation
        // await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, plan);
    }

    [RelayCommand]
    private async Task NavigateToHomeAsync()
    {
        try
        {
            await _navigator.NavigateBackAsync(this).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigationsfehler: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddPlan()
    {
        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());
        Trainingsplaene.Add(neuerPlan);
    }
}
