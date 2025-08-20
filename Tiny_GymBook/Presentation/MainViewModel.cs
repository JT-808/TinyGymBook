using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiny_GymBook.Models;
using Tiny_GymBook.Services.DataService;
using System.Linq;

namespace Tiny_GymBook.Presentation;

[Bindable]
public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IDataService _trainingsplanDBService;

    [ObservableProperty]
    private Trainingswoche? aktuelleWoche;

    [ObservableProperty]
    private Trainingsplan? aktiverPlan;

    public ObservableCollection<Trainingsplan> AllePlaene { get; } = new();
    public ObservableCollection<Tag> AlleTage { get; } = new(); // HIER: Alle Tage + Übungen + Sätze!

    public ObservableCollection<Trainingswoche> Wochen { get; } = new();

    public MainViewModel(INavigator navigator, IDataService trainingsplanService)
    {
        _navigator = navigator;
        _trainingsplanDBService = trainingsplanService;

        _ = _trainingsplanDBService.InitAsync();

        InitializeWeek();            // 1) Woche bestimmen
        _ = LadeAllePlaeneAsync();   // 2) Pläne laden -> setzt AktiverPlan

        // 3) Initial Wochen laden (OnAktiverPlanChanged ruft es eh nochmal – doppelt ist ok, aber optional)
        _ = LoadInitialWeeksAsync();
    }


    private async Task LadeAllePlaeneAsync()
    {
        var plaene = await _trainingsplanDBService.LadeTrainingsplaeneAsync();
        AllePlaene.Clear();
        foreach (var plan in plaene)
            AllePlaene.Add(plan);

        if (AktiverPlan == null && AllePlaene.Any())
            AktiverPlan = AllePlaene.First();
    }

    private void InitializeWeek()
    {
        var heute = DateTime.Today;

        //var heute = new DateTime(2025, 9, 01); // <<< Testdatum

        int offset = ((int)heute.DayOfWeek + 6) % 7; // Mo=0, So=6
        var montag = heute.AddDays(-offset);
        var kw = System.Globalization.ISOWeek.GetWeekOfYear(heute);
        AktuelleWoche = new Trainingswoche(kw, heute.Year, montag);
    }

    // Immer wenn sich der aktive Plan ändert, lade die Tage/Übungen/Sätze neu!
    partial void OnAktiverPlanChanged(Trainingsplan? value)
    {
        _ = RebuildCurrentWeekFromActivePlanAsync();
    }




    [RelayCommand]
    public async Task AddSatzAsync(Uebung uebung)
    {
        if (uebung == null) return;

        // Finde die Woche, in deren Struktur diese Uebung-Instanz steckt
        var zielWoche = Wochen.FirstOrDefault(w =>
            w.Tage.Any(t => t.Uebungen.Contains(uebung)));
        if (zielWoche == null) return;

        // Max Nummer der *Zielwoche*
        var bestehende = await _trainingsplanDBService
            .LadeSaetzeFuerUebungInWocheAsync(uebung.Uebung_Id, zielWoche.Jahr, zielWoche.KalenderWoche);
        int neueNummer = (bestehende.Count == 0) ? 1 : bestehende.Max(s => s.Nummer) + 1;

        var s = new Satz
        {
            Nummer = neueNummer,
            Uebung_Id = uebung.Uebung_Id,
            Gewicht = 0,
            Wiederholungen = 0,
            Kommentar = string.Empty,
            Training_Date = DateTime.Today.ToString("yyyy-MM-dd"),
            Jahr = zielWoche.Jahr,
            KalenderWoche = zielWoche.KalenderWoche
        };

        uebung.Saetze.Add(s);
        await _trainingsplanDBService.SpeichereSaetzeFuerUebungInWocheAsync(
            uebung.Uebung_Id, zielWoche.Jahr, zielWoche.KalenderWoche, uebung.Saetze);
    }

    [RelayCommand]
    private async Task NavigateToPlaeneAsync()
    {
        foreach (var woche in Wochen)
            foreach (var tag in woche.Tage)
                foreach (var uebung in tag.Uebungen)
                    await _trainingsplanDBService.SpeichereSaetzeFuerUebungInWocheAsync(
                        uebung.Uebung_Id, woche.Jahr, woche.KalenderWoche, uebung.Saetze);

        await _navigator.NavigateViewModelAsync<SecondViewModel>(this);
    }




    [RelayCommand]
    public async Task LoadInitialWeeksAsync()
    {
        Wochen.Clear();
        if (AktuelleWoche is null)
            return;

        if (AktiverPlan is not null)
        {
            // aktuelle Woche = Struktur des aktiven Plans
            var w0 = await BuildWeekAsync(AktuelleWoche.Jahr, AktuelleWoche.KalenderWoche, AktiverPlan.Trainingsplan_Id);
            Wochen.Add(w0);
        }
        else
        {
            // Fallback: aggregierte Ansicht
            var w0 = await BuildWeekAsync(AktuelleWoche.Jahr, AktuelleWoche.KalenderWoche);
            if (w0.Tage.Any())
                Wochen.Add(w0);
        }
    }

    [RelayCommand]
    public async Task LoadOlderWeekAsync()
    {
        if (Wochen.Count == 0)
        {
            if (AktuelleWoche is null) return;
            var w0 = (AktiverPlan is not null)
                ? await BuildWeekAsync(AktuelleWoche.Jahr, AktuelleWoche.KalenderWoche, AktiverPlan.Trainingsplan_Id)
                : await BuildWeekAsync(AktuelleWoche.Jahr, AktuelleWoche.KalenderWoche);

            if (w0.Tage.Any()) Wochen.Add(w0);
            return;
        }

        var top = Wochen.First();
        var prevMonday = System.Globalization.ISOWeek
            .ToDateTime(top.Jahr, top.KalenderWoche, DayOfWeek.Monday)
            .AddDays(-7);

        var pj = System.Globalization.ISOWeek.GetYear(prevMonday);
        var pkw = System.Globalization.ISOWeek.GetWeekOfYear(prevMonday);

        if (Wochen.Any(w => w.Jahr == pj && w.KalenderWoche == pkw))
            return;

        // ältere Wochen = aggregierte Historie
        var woche = await BuildWeekAsync(pj, pkw);
        if (woche.Tage.Any())
            Wochen.Insert(0, woche);
    }


    private async Task<Trainingswoche> BuildWeekAsync(int jahr, int kw, int? planIdFilter = null)
    {
        var montag = System.Globalization.ISOWeek.ToDateTime(jahr, kw, DayOfWeek.Monday);
        var woche = new Trainingswoche(kw, jahr, montag);

        if (planIdFilter.HasValue)
        {
            // —— Plan-spezifische Ansicht für diese Woche —— //
            var tage = await _trainingsplanDBService.LadeTageAsync(planIdFilter.Value);
            var uebungenPlan = await _trainingsplanDBService.LadeUebungenZuPlanAsync(planIdFilter.Value);

            foreach (var t in tage.OrderBy(t => t.Reihenfolge))
            {
                var tagVm = new Tag
                {
                    Name = t.Name,
                    Reihenfolge = t.Reihenfolge,
                    Trainingsplan_Id = t.Trainingsplan_Id
                };

                foreach (var u in uebungenPlan.Where(u => u.TagId == t.TagId).OrderBy(u => u.Name))
                {
                    // Falls es schon Sätze dieser Übung in dieser Woche gibt, laden und anhängen:
                    var saetze = await _trainingsplanDBService
                        .LadeSaetzeFuerUebungInWocheAsync(u.Uebung_Id, jahr, kw);

                    tagVm.Uebungen.Add(new Uebung
                    {
                        Uebung_Id = u.Uebung_Id,
                        Name = u.Name,
                        Muskelgruppe = u.Muskelgruppe,
                        TagId = u.TagId,
                        Trainingsplan_Id = u.Trainingsplan_Id,
                        Saetze = new ObservableCollection<Satz>(saetze.OrderBy(s => s.Nummer))
                    });
                }

                woche.Tage.Add(tagVm);
            }

            return woche;
        }

        // —— Aggregierte Historie (deine bestehende Logik) —— //
        var saetzeWoche = await _trainingsplanDBService.LadeSaetzeInWocheAsync(jahr, kw);
        if (saetzeWoche.Count == 0)
            return woche;

        var uebungIds = saetzeWoche.Select(s => s.Uebung_Id).Distinct().ToList();
        var uebungen = await _trainingsplanDBService.LadeUebungenByIdsAsync(uebungIds);
        var uDict = uebungen.ToDictionary(u => u.Uebung_Id);

        var tagIds = uebungen.Select(u => u.TagId).Distinct().ToList();
        var tags = await _trainingsplanDBService.LadeTagsByIdsAsync(tagIds);
        var tDict = tags.ToDictionary(t => t.TagId);

        var planIds = tags.Select(t => t.Trainingsplan_Id).Distinct().ToList();
        var plaene = await _trainingsplanDBService.LadePlaeneByIdsAsync(planIds);
        var pDict = plaene.ToDictionary(p => p.Trainingsplan_Id);

        var saetzeByUebung = saetzeWoche.GroupBy(s => s.Uebung_Id);
        var uebungenByTag = new Dictionary<int, List<Uebung>>();

        foreach (var grp in saetzeByUebung)
        {
            if (!uDict.TryGetValue(grp.Key, out var uMeta)) continue;
            if (!tDict.TryGetValue(uMeta.TagId, out var tagMeta)) continue;

            var uVm = new Uebung
            {
                Uebung_Id = uMeta.Uebung_Id,
                Name = uMeta.Name,
                Muskelgruppe = uMeta.Muskelgruppe,
                TagId = uMeta.TagId,
                Trainingsplan_Id = uMeta.Trainingsplan_Id,
                Saetze = new ObservableCollection<Satz>(grp.OrderBy(s => s.Nummer))
            };

            if (!uebungenByTag.TryGetValue(uMeta.TagId, out var list))
                uebungenByTag[uMeta.TagId] = list = new List<Uebung>();

            list.Add(uVm);
        }

        foreach (var tagId in uebungenByTag.Keys.OrderBy(id => tDict[id].Reihenfolge))
        {
            var tagMeta = tDict[tagId];
            var planName = pDict.TryGetValue(tagMeta.Trainingsplan_Id, out var plan)
                ? plan.Name
                : $"Plan {tagMeta.Trainingsplan_Id}";

            var tagVm = new Tag
            {
                Name = $"{planName} • {tagMeta.Name}",
                Reihenfolge = tagMeta.Reihenfolge,
                Trainingsplan_Id = tagMeta.Trainingsplan_Id
            };

            foreach (var u in uebungenByTag[tagId].OrderBy(x => x.Name))
                tagVm.Uebungen.Add(u);

            woche.Tage.Add(tagVm);
        }

        return woche;
    }

    [RelayCommand]
    public async Task ApplyPlanToCurrentWeekAsync()
    {
        if (AktuelleWoche is null || AktiverPlan is null) return;

        var neueWoche = await BuildWeekAsync(AktuelleWoche.Jahr, AktuelleWoche.KalenderWoche, AktiverPlan.Trainingsplan_Id);

        // Ersetze die aktuelle Woche (falls schon vorhanden) oder füge sie hinzu
        var exist = Wochen.FirstOrDefault(w => w.Jahr == neueWoche.Jahr && w.KalenderWoche == neueWoche.KalenderWoche);
        if (exist is not null)
        {
            var idx = Wochen.IndexOf(exist);
            Wochen[idx] = neueWoche;
        }
        else
        {
            Wochen.Add(neueWoche);
        }
    }


    private async Task RebuildCurrentWeekFromActivePlanAsync()
    {
        if (AktuelleWoche is null || AktiverPlan is null) return;

        var wNeu = await BuildWeekAsync(AktuelleWoche.Jahr, AktuelleWoche.KalenderWoche, AktiverPlan.Trainingsplan_Id);

        // vorhandene aktuelle Woche im Feed ersetzen oder hinzufügen
        var exist = Wochen.FirstOrDefault(w => w.Jahr == wNeu.Jahr && w.KalenderWoche == wNeu.KalenderWoche);
        if (exist is not null)
        {
            var idx = Wochen.IndexOf(exist);
            Wochen[idx] = wNeu;
        }
        else
        {
            Wochen.Add(wNeu);
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigator.NavigateBackAsync(this);
        Debug.WriteLine("[DEBUG] Von Home auf Home gewechselt");
    }


}
