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
    private Trainingsplan? trainingsplan;

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

    // ****** Buttons ********//

    [RelayCommand]
    private async Task AddPlanAsync()
    {
        // Plan erzeugen und speichern
        var neuerPlan = new Trainingsplan("Neuer Plan", new List<Uebung>());
        await _trainingsplanService.SpeichereTrainingsplanAsync(neuerPlan, new List<Tag>());

        // Pläne neu laden
        await LadeTrainingsplaeneAsync();

        // Jetzt die *aktuelle* Instanz (mit ID!) holen
        var insertedPlan = Trainingsplaene.OrderByDescending(p => p.Trainingsplan_Id).FirstOrDefault();

        if (insertedPlan != null)
        {
            Debug.WriteLine($"[DEBUG] Navigiere direkt zum neuen Plan: {insertedPlan.Name}, ID: {insertedPlan.Trainingsplan_Id}");

            // Jetzt korrekt navigieren
            await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: insertedPlan);
        }
    }

    [RelayCommand]
    private async Task DeletePlanAsync(Trainingsplan plan)
    {
        if (plan is null) return;

        await _trainingsplanService.LoescheTrainingsplanAsync(plan);
        await LadeTrainingsplaeneAsync(); // <-- jetzt garantiert aktuell
    }

    [RelayCommand]
    private async Task OpenPlanAsync(Trainingsplan plan)
    {
        Debug.WriteLine($"[DEBUG] Navigiere zu PlanDetailViewModel mit Plan: {plan?.Name}, ID: {plan?.Trainingsplan_Id}");
        await _navigator.NavigateViewModelAsync<PlanDetailViewModel>(this, data: plan);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
