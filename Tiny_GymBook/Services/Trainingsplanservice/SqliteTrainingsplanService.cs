using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.Trainingsplanservice;

public class SqliteTrainingsplanService : ITrainingsplanService
{
    private readonly SQLiteAsyncConnection _db;

    public SqliteTrainingsplanService()
    {
        // Plattformunabhängiger Pfad im Benutzerordner:
        var ordner = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Tiny_GymBook"
        );
        Directory.CreateDirectory(ordner);
        var dbPfad = Path.Combine(ordner, "trainingsplaene.db");

        // Debug: Ausgabe des Datenbankpfads
        Console.WriteLine("[DEBUG] SQLite-Pfad: " + dbPfad);

        _db = new SQLiteAsyncConnection(dbPfad);
    }
    private bool _initialized; //false
    public async Task InitAsync()
    {
        try
        {
            //Verhindere Double-init
            if (_initialized) return;
            _initialized = true;

            await _db.CreateTableAsync<Trainingsplan>();
            await _db.CreateTableAsync<Uebung>();
            await _db.CreateTableAsync<Tag>();
            await _db.CreateTableAsync<Satz>();

            // Index (optional, schneller bei Abfragen):
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_satz_uebung_nummer ON Satz(Uebung_Id, Nummer)");
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine("[SQLite][InitAsync] " + ex.Message);
            throw;
        }
    }


    public async Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync()
    {
        var plaene = await _db.Table<Trainingsplan>().ToListAsync();
        var alleUebungen = await _db.Table<Uebung>().ToListAsync();
        foreach (var plan in plaene)
        {
            plan.Uebungen = new ObservableCollection<Uebung>(
                alleUebungen.Where(u => u.Trainingsplan_Id == plan.Trainingsplan_Id)
            );
            Debug.WriteLine($"[DEBUG] Plan: {plan.Name}, Id={plan.Trainingsplan_Id}, Anzahl Übungen: {plan.Uebungen.Count}");
            foreach (var u in plan.Uebungen)
            {
                Debug.WriteLine($"    [DEBUG] Übung: {u.Name}, Id={u.Uebung_Id}, PlanId={u.Trainingsplan_Id}");
            }
        }
        return plaene;
    }

    public async Task SpeichereTrainingsplanAsync(Trainingsplan plan, IEnumerable<Tag> tage)
    {
        if (plan.Trainingsplan_Id == 0)
        {
            await _db.InsertAsync(plan);
            Debug.WriteLine($"[DEBUG] Insert Plan: ID now {plan.Trainingsplan_Id}");
        }
        else
        {
            var result = await _db.UpdateAsync(plan);
            Debug.WriteLine($"[DEBUG] UpdateAsync RESULT: {result}");
        }



        // Nach dem Insert: Wenn Plan ganz neu, Tag 1 anlegen
        if (plan.Trainingsplan_Id != 0 && !tage.Any())
        {
            var ersterTag = new Tag
            {
                Name = "Tag 1",
                Reihenfolge = 1,
                Trainingsplan_Id = plan.Trainingsplan_Id
            };
            await SpeichereTagAsync(ersterTag);
        }


        foreach (var tag in tage)
        {
            tag.Trainingsplan_Id = plan.Trainingsplan_Id;
            if (tag.TagId == 0)
            {
                await _db.InsertAsync(tag);
                Debug.WriteLine($"[DEBUG] Tag nach Insert: {tag.Name}, Id={tag.TagId}");
            }
            else
            {
                await _db.UpdateAsync(tag);
                Debug.WriteLine($"[DEBUG] Tag nach Update: {tag.Name}, Id={tag.TagId}");
            }

            // Speichere die Übungen dieses Tags!
            foreach (var uebung in tag.Uebungen)
            {
                uebung.Trainingsplan_Id = plan.Trainingsplan_Id;
                uebung.TagId = tag.TagId;
                if (uebung.Uebung_Id == 0)
                    await _db.InsertAsync(uebung);
                else
                    await _db.UpdateAsync(uebung);
            }
        }
    }


    public async Task LoescheTrainingsplanAsync(Trainingsplan plan)
    {
        await _db.DeleteAsync(plan);
    }


    public async Task<List<Uebung>> LadeUebungenZuPlanAsync(int trainingsplanId)
    {
        return await _db.Table<Uebung>()
            .Where(u => u.Trainingsplan_Id == trainingsplanId)
            .ToListAsync();
    }

    public async Task SpeichereUebung(Uebung uebung)
    {
        if (uebung.Uebung_Id == 0)
        {
            await _db.InsertAsync(uebung);
            Debug.WriteLine($"[DEBUG] Einzelübung gespeichert: {uebung.Name}, Id={uebung.Uebung_Id}, PlanId={uebung.Trainingsplan_Id}");
        }
        else
        {
            await _db.UpdateAsync(uebung);
            Debug.WriteLine($"[DEBUG] Einzelübung aktualisiert: {uebung.Name}, Id={uebung.Uebung_Id}, PlanId={uebung.Trainingsplan_Id}");
        }
    }


    public async Task SpeichereTagAsync(Tag tag)
    {
        if (tag.TagId == 0)
        {
            await _db.InsertAsync(tag);
            Debug.WriteLine($"[DEBUG] Tag gespeichert: Id={tag.TagId}, Name={tag.Name}");
        }
        else
        {
            await _db.UpdateAsync(tag);
            Debug.WriteLine($"[DEBUG] Tag aktualisiert: Id={tag.TagId}, Name={tag.Name}");
        }
    }


    public async Task<List<Tag>> LadeTageAsync(int trainingsplanId)
    {
        // Hole nur Tage für den angegebenen Trainingsplan
        return await _db.Table<Tag>()
            .Where(t => t.Trainingsplan_Id == trainingsplanId)
            .OrderBy(t => t.Reihenfolge)
            .ToListAsync();
    }

    // Nach umstruktierung
    public async Task<List<Satz>> LadeSaetzeFuerUebungAsync(int uebungId)
    {
        return await _db.Table<Satz>()
                        .Where(s => s.Uebung_Id == uebungId)
                        .OrderBy(s => s.Nummer)
                        .ToListAsync();
    }

    public async Task SpeichereSaetzeFuerUebungAsync(int uebungId, IEnumerable<Satz> saetze)
    {
        // Bestehende Sätze holen
        var bestehende = await _db.Table<Satz>()
                                  .Where(s => s.Uebung_Id == uebungId)
                                  .ToListAsync();

        // Löschen, was entfernt wurde
        var aktuelleIds = saetze.Where(s => s.Satz_Id != 0).Select(s => s.Satz_Id).ToHashSet();
        foreach (var alt in bestehende)
        {
            if (!aktuelleIds.Contains(alt.Satz_Id))
                await _db.DeleteAsync(alt);
        }

        // Einfügen/Aktualisieren
        foreach (var satz in saetze)
        {
            satz.Uebung_Id = uebungId; // FK sicher setzen

            if (satz.Satz_Id == 0)
                await _db.InsertAsync(satz);
            else
                await _db.UpdateAsync(satz);
        }
    }







}
