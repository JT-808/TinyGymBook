using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Uno.Extensions.Navigation;
using Tiny_GymBook.Services.DataService;

using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Presentation;


[Bindable]
public partial class PlanDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IDataService _trainingsplanDBService;

    [ObservableProperty]
    private Trainingsplan? trainingsplan;

    public ObservableCollection<Tag> Tage { get; } = new();


    public PlanDetailViewModel(INavigator navigator, IDataService trainingsplanService, Trainingsplan plan)
    {
        Debug.WriteLine("[DEBUG] PlanDetailViewModel Konstruktor aufgerufen!");

        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanDBService = trainingsplanService ?? throw new ArgumentNullException(nameof(trainingsplanService));
        Trainingsplan = plan;

        _ = LadeTageUndUebungenAsync();
    }

    public async Task LadeTageUndUebungenAsync()
    {
        if (Trainingsplan is null) return;

        var tageAusDb = await _trainingsplanDBService.LadeTageAsync(Trainingsplan.Trainingsplan_Id);
        var uebungenAusDb = await _trainingsplanDBService.LadeUebungenZuPlanAsync(Trainingsplan.Trainingsplan_Id);

        Tage.Clear();

        foreach (var tag in tageAusDb)
        {
            tag.Uebungen.Clear();
            var uebungenFuerTag = uebungenAusDb.Where(u => u.TagId == tag.TagId); //
            foreach (var u in uebungenFuerTag)
                tag.Uebungen.Add(u);

            Tage.Add(tag);
        }
    }

    // ****** Buttons ********//

    [RelayCommand]
    public async Task AddUebungToTagAsync(Tag tag)
    {
        var uebung = Uebung.CreateNew(Trainingsplan?.Trainingsplan_Id ?? 0, tag.TagId);
        await _trainingsplanDBService.SpeichereUebung(uebung);
        tag.Uebungen.Add(uebung);
    }

    [RelayCommand]
    public async Task AddTagAsync()
    {
        var nextNr = Tage.Count + 1;
        var newTag = new Tag
        {
            Name = $"Tag {nextNr}",
            Reihenfolge = nextNr,
            Trainingsplan_Id = Trainingsplan?.Trainingsplan_Id ?? 0
        };
        await _trainingsplanDBService.SpeichereTagAsync(newTag);
        Tage.Add(newTag);
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
            await _trainingsplanDBService.SpeichereTrainingsplanAsync(Trainingsplan, Tage);
            await _navigator.NavigateBackAsync(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FEHLER] Back-Navigation: {ex.Message}");
        }
    }
}
