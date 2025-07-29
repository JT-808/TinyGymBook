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
            Trainingsplaene.Add(plan);
        }
    }

    [RelayCommand]
    private async Task AddPlanAsync()
    {
        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());
        Trainingsplaene.Add(neuerPlan);
        await _trainingsplanService.SpeichereAlleTrainingsplaeneAsync(Trainingsplaene);
    }

    [RelayCommand]
    private async Task OpenPlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;

        Debug.WriteLine($"[DEBUG] Öffne Plan: {plan.Name}");

        //  await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this,data: plan);
    }

    [RelayCommand]
    private async Task NavigateToHomeAsync()
    {
        try
        {
            await _navigator.NavigateBackAsync(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FEHLER] Navigation: {ex.Message}");
        }
    }
}
