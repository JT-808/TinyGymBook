using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Tiny_GymBook.Services.Trainingsplanservice;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class SecondViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly ITrainingsplanService _trainingsplanService;

    [ObservableProperty]
    private ObservableCollection<Trainingsplan> trainingsplaene = new();

    [ObservableProperty]
    private Trainingsplan? selectedPlan;

    public SecondViewModel(INavigator navigator, ITrainingsplanService trainingsplanService)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanService = trainingsplanService ?? throw new ArgumentNullException(nameof(trainingsplanService));

        Debug.WriteLine($"[DEBUG] Navigator initialisiert: {_navigator.GetType().Name}");

        _ = LadeTrainingsplaeneAsync();
    }

    private async Task LadeTrainingsplaeneAsync()
    {
        Debug.WriteLine("[DEBUG] Starte LadeTrainingsplaeneAsync()");
        var geladenePlaene = await _trainingsplanService.LadeTrainingsplaeneAsync();
        Trainingsplaene.Clear();
        foreach (var plan in geladenePlaene)
        {
            Debug.WriteLine($"[DEBUG] Geladen: {plan.Name} mit {plan.Uebungen.Count} Übungen");
            Debug.WriteLine($"[DEBUG] DB-Plan: {plan.Trainingsplan_Id}, Name = {plan.Name}");
            Trainingsplaene.Add(plan);
        }
    }

    [RelayCommand]
    private async Task AddPlanAsync()
    {
        var count = await _trainingsplanService.LadeTrainingsplaeneAsync();
        Debug.WriteLine($"[DEBUG] Es gibt jetzt {count.Count()} Pläne in der DB.");

        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());

        await _trainingsplanService.SpeichereTrainingsplanAsync(neuerPlan);

        // Die ID sollte jetzt automatisch gesetzt sein
        Debug.WriteLine($"[DEBUG] Neuer Plan nach Insert: ID = {neuerPlan.Trainingsplan_Id}");

        await LadeTrainingsplaeneAsync();

        var count2 = await _trainingsplanService.LadeTrainingsplaeneAsync();
        Debug.WriteLine($"[DEBUG] Es gibt jetzt {count2.Count()} Pläne in der DB.");
    }

    [RelayCommand]
    private async Task DeletePlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;

        await _trainingsplanService.LoescheTrainingsplanAsync(plan);
        Trainingsplaene.Remove(plan);
    }

    [RelayCommand]
    private async Task OpenPlanAsync(Trainingsplan plan)
    {
        Debug.WriteLine($"[DEBUG] OpenPlanAsync ausgeführt für Plan: {plan?.Name}");
        if (plan is null) return;
        // Navigation zur PlanDetail-Seite, Daten als Parameter übergeben
        await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: plan);
    }

    [RelayCommand]
    private async Task NavigateToHomeAsync()
    {
        try
        {
            await _navigator.NavigateBackAsync(this); //TODO Später: Seiten direkt ansprechen
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FEHLER] Navigation: {ex.Message}");
        }
    }
}
