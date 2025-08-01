using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Tiny_GymBook.Services.Trainingsplanservice;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class PlanDetailViewModel : ObservableObject
{

    private readonly INavigator _navigator;

    [ObservableProperty]
    private Trainingsplan trainingsplan;
    private readonly ITrainingsplanService _trainingsplanService;


    public ObservableCollection<Uebung> Uebungen { get; } = new();

    public ObservableCollection<Trainingseintrag> AlleEintraege { get; set; } = new();

    // Für das UI gruppiert nach Tag:
    public ObservableCollection<ObservableCollection<Trainingseintrag>> Tage { get; set; } = new();

    public IEnumerable<IGrouping<string, Trainingseintrag>> GruppierteEintraege
     => AlleEintraege.GroupBy(e => e.Tag);

    public PlanDetailViewModel(INavigator navigator, ITrainingsplanService trainingsplanService)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _trainingsplanService = trainingsplanService ?? throw new ArgumentNullException(nameof(trainingsplanService));

        // Beispiel-Daten
        AlleEintraege.Add(new Trainingseintrag(new Uebung { Name = "Bankdrücken" }) { Tag = "Tag 1" });
        AlleEintraege.Add(new Trainingseintrag(new Uebung { Name = "Schrägbankdrücken" }) { Tag = "Tag 1" });
        AlleEintraege.Add(new Trainingseintrag(new Uebung { Name = "Kniebeugen" }) { Tag = "Tag 2" });
    }



    [RelayCommand]
    private void AddUebung()
    {
        Uebungen.Add(new Uebung { Name = "Neue Übung" });
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


    //Naviagation -> Soll später in die Shell

    [RelayCommand]
    private async Task SaveAndGoBackAsync(Trainingsplan plan)
    {
        try
        {
            await _trainingsplanService.SpeichereTrainingsplanAsync(plan);
            await _navigator.NavigateBackAsync(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FEHLER] Back-Navigation: {ex.Message}");
        }
    }



}
