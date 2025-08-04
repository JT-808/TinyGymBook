using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Uno.Extensions.Navigation;
using Tiny_GymBook.Services.Trainingsplanservice;

using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class PlanDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly ITrainingsplanService _trainingsplanService;

    [ObservableProperty]
    private Trainingsplan? trainingsplan;

    public ObservableCollection<Uebung>? Uebungen => Trainingsplan?.Uebungen;
    public ObservableCollection<Trainingseintrag> AlleEintraege { get; } = new();

    public IEnumerable<IGrouping<string, Trainingseintrag>> GruppierteEintraege
        => AlleEintraege.GroupBy(e => e.Tag);

    public PlanDetailViewModel(INavigator navigator, ITrainingsplanService trainingsplanService, Trainingsplan plan)
    {
        Debug.WriteLine("[DEBUG] PlanDetailViewModel Konstruktor aufgerufen!");

        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanService = trainingsplanService ?? throw new ArgumentNullException(nameof(trainingsplanService));
        Trainingsplan = plan;
    }



    partial void OnTrainingsplanChanged(Trainingsplan? value)
    {
        Debug.WriteLine("[DEBUG] OnTrainingsplanChanged wurde aufgerufen!");
        Debug.WriteLine($"[DEBUG] Trainingsplan gesetzt: {value?.Name} / ID: {value?.Trainingsplan_Id}");
        OnPropertyChanged(nameof(Uebungen));
    }



    [RelayCommand]
    private async Task AddUebungDBAsync()
    {
        if (Trainingsplan == null)
            return;

        var neueUebung = new Uebung
        {
            Name = "Neue Übung",
            Trainingsplan_Id = Trainingsplan.Trainingsplan_Id
        };

        await _trainingsplanService.SpeichereUebung(neueUebung);
        Trainingsplan.Uebungen.Add(neueUebung);
    }

    [RelayCommand]
    public void AddUebungToTag(string tag)
    {
        AlleEintraege.Add(new Trainingseintrag { Tag = tag, Uebung = new Uebung { Name = "Neue Übung" } });
        OnPropertyChanged(nameof(GruppierteEintraege));
    }

    [RelayCommand]
    public void AddTag()
    {
        var tagNum = GruppierteEintraege.Count() + 1;
        AlleEintraege.Add(new Trainingseintrag { Tag = $"Tag {tagNum}", Uebung = new Uebung { Name = "Neue Übung" } });
        OnPropertyChanged(nameof(GruppierteEintraege));
    }

    [RelayCommand]
    private async Task SaveAndGoBackAsync()
    {
        if (Trainingsplan == null)
        {
            Debug.WriteLine("[FEHLER] Trainingsplan ist null! (SaveAndGoBackAsync)");
            return;
        }
        try
        {
            await _trainingsplanService.SpeichereTrainingsplanAsync(Trainingsplan);
            await _navigator.NavigateBackAsync(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FEHLER] Back-Navigation: {ex.Message}");
        }
    }
}
