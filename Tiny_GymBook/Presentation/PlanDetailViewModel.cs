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
    public ObservableCollection<Tag> Tage { get; } = new();
    public ObservableCollection<Trainingseintrag> AlleEintraege { get; } = new();


    public PlanDetailViewModel(INavigator navigator, ITrainingsplanService trainingsplanService, Trainingsplan plan)
    {
        Debug.WriteLine("[DEBUG] PlanDetailViewModel Konstruktor aufgerufen!");

        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanService = trainingsplanService ?? throw new ArgumentNullException(nameof(trainingsplanService));
        Trainingsplan = plan;

        // Immer mindestens "Tag 1" anlegen
        if (!Tage.Any())
            Tage.Add(new Tag { Name = "Tag 1", Reihenfolge = 1 });


        _ = LadeTageUndEintraegeAsync();
    }

    partial void OnTrainingsplanChanged(Trainingsplan? value)
    {
        Debug.WriteLine("[DEBUG] OnTrainingsplanChanged wurde aufgerufen!");
        Debug.WriteLine($"[DEBUG] Trainingsplan gesetzt: {value?.Name} / ID: {value?.Trainingsplan_Id}");
        OnPropertyChanged(nameof(Uebungen));
    }


    public async Task LadeTageUndEintraegeAsync()
    {
        if (Trainingsplan is null) return;

        var tageAusDb = await _trainingsplanService.LadeTageAsync(Trainingsplan.Trainingsplan_Id);
        var eintraegeAusDb = await _trainingsplanService.LadeTrainingseintraegeAsync(Trainingsplan.Trainingsplan_Id);

        Tage.Clear();
        AlleEintraege.Clear();

        foreach (var tag in tageAusDb)
        {
            tag.Eintraege.Clear();
            var zugeordneteEintraege = eintraegeAusDb.Where(e => e.TagId == tag.TagId).ToList();
            foreach (var eintrag in zugeordneteEintraege)
                tag.Eintraege.Add(eintrag);

            Tage.Add(tag);
        }
        foreach (var eintrag in eintraegeAusDb)
            AlleEintraege.Add(eintrag);

        OnPropertyChanged(nameof(Tage));
        OnPropertyChanged(nameof(AlleEintraege));
    }

    // Gib Einträge für einen bestimmten Tag
    // Für später! (Wird aktuell nicht genutzt)
    public IEnumerable<Trainingseintrag> GetEintraegeFuerTag(Tag tag)
        => AlleEintraege.Where(e => e.TagId == tag.TagId);

    //
    // ********************** Buttons *********************************
    //

    [RelayCommand]
    public async Task AddUebungToTagAsync(Tag tag)
    {
        // Hier solltest du ggf. eine echte Übung referenzieren/auswählen!
        var uebung = new Uebung { Name = "Neue Übung" };

        var eintrag = new Trainingseintrag
        {
            TagId = tag.TagId,
            Uebung_Id = uebung.Uebung_Id,
            Trainingsplan_Id = Trainingsplan?.Trainingsplan_Id ?? 0,
            // Weitere Felder falls nötig
        };

        await _trainingsplanService.SpeichereTrainingseintragAsync(eintrag);
        AlleEintraege.Add(eintrag);

        // NEU: Auch im Tag hinzufügen!
        tag.Eintraege.Add(eintrag);
        // Falls du auf ObservableCollection setzt, reicht das schon!
        OnPropertyChanged(nameof(AlleEintraege));
        OnPropertyChanged(nameof(Tage));
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
        await _trainingsplanService.SpeichereTagAsync(newTag);
        Tage.Add(newTag);
        OnPropertyChanged(nameof(Tage));
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
