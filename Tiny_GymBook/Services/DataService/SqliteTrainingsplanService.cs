using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.DataService;

public class SqliteTrainingsplanService : IDataService
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

            // Spalten nachrüsten (schadet nicht, wenn sie schon da sind)
            try { await _db.ExecuteAsync("ALTER TABLE Satz ADD COLUMN Jahr INTEGER DEFAULT 0"); } catch { }
            try { await _db.ExecuteAsync("ALTER TABLE Satz ADD COLUMN KalenderWoche INTEGER DEFAULT 0"); } catch { }

            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_uebung_tag ON Uebung(TagId)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_tag_plan ON Tag(Trainingsplan_Id)");

            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_satz_uebung_nummer ON Satz(Uebung_Id, Nummer)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_satz_week ON Satz(Jahr, KalenderWoche, Uebung_Id)");
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
                Name = string.Empty,
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

    public async Task DeleteTagAsync(int tagId)
    {
        await _db.RunInTransactionAsync(conn =>
        {
            // Übungen zu diesem Tag holen
            var uebungen = conn.Table<Uebung>().Where(u => u.TagId == tagId).ToList();

            // Zuerst alle Sätze der Übungen löschen
            foreach (var u in uebungen)
                conn.Execute("DELETE FROM Satz WHERE Uebung_Id = ?", u.Uebung_Id);

            // Dann die Übungen selbst löschen
            conn.Execute("DELETE FROM Uebung WHERE TagId = ?", tagId);

            // Zuletzt den Tag löschen
            conn.Execute("DELETE FROM Tag WHERE TagId = ?", tagId);
        });
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


    public async Task<List<Satz>> LadeSaetzeFuerUebungInWocheAsync(int uebungId, int jahr, int kw)
    {
        return await _db.Table<Satz>()
            .Where(s => s.Uebung_Id == uebungId && s.Jahr == jahr && s.KalenderWoche == kw)
            .OrderBy(s => s.Nummer)
            .ToListAsync();
    }

    public async Task SpeichereSaetzeFuerUebungInWocheAsync(int uebungId, int jahr, int kw, IEnumerable<Satz> saetze)
    {
        // Nur Sätze der *gleichen Woche* betrachten
        var bestehende = await _db.Table<Satz>()
                                  .Where(s => s.Uebung_Id == uebungId && s.Jahr == jahr && s.KalenderWoche == kw)
                                  .ToListAsync();

        var aktuelleIds = saetze.Where(s => s.Satz_Id != 0).Select(s => s.Satz_Id).ToHashSet();

        // Löschen, was in *dieser Woche* entfernt wurde
        foreach (var alt in bestehende)
            if (!aktuelleIds.Contains(alt.Satz_Id))
                await _db.DeleteAsync(alt);

        // Einfügen/Aktualisieren – dabei Jahr/KW konsistent setzen
        foreach (var satz in saetze)
        {
            satz.Uebung_Id = uebungId;
            satz.Jahr = jahr;
            satz.KalenderWoche = kw;

            if (satz.Satz_Id == 0) await _db.InsertAsync(satz);
            else await _db.UpdateAsync(satz);
        }
    }



    public async Task<List<Satz>> LadeSaetzeInWocheAsync(int jahr, int kw)
    {
        return await _db.Table<Satz>()
            .Where(s => s.Jahr == jahr && s.KalenderWoche == kw)
            .OrderBy(s => s.Uebung_Id).ThenBy(s => s.Nummer)
            .ToListAsync();
    }




    public async Task<List<Uebung>> LadeUebungenByIdsAsync(IEnumerable<int> uebungIds)
    {
        var ids = uebungIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0) return new List<Uebung>();

        var qm = string.Join(",", ids.Select(_ => "?"));
        var sql = $"SELECT * FROM Uebung WHERE Uebung_Id IN ({qm})";
        return await _db.QueryAsync<Uebung>(sql, ids.Cast<object>().ToArray());
    }

    public async Task<List<Tag>> LadeTagsByIdsAsync(IEnumerable<int> tagIds)
    {
        var ids = tagIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0) return new List<Tag>();

        var qm = string.Join(",", ids.Select(_ => "?"));
        var sql = $"SELECT * FROM Tag WHERE TagId IN ({qm})";
        return await _db.QueryAsync<Tag>(sql, ids.Cast<object>().ToArray());
    }

    public async Task<List<Trainingsplan>> LadePlaeneByIdsAsync(IEnumerable<int> planIds)
    {
        var ids = planIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0) return new List<Trainingsplan>();

        var qm = string.Join(",", ids.Select(_ => "?"));
        var sql = $"SELECT * FROM Trainingsplan WHERE Trainingsplan_Id IN ({qm})";
        return await _db.QueryAsync<Trainingsplan>(sql, ids.Cast<object>().ToArray());
    }


    public async Task<int> ImportTrainingsplaeneAsync(IEnumerable<Trainingsplan> plaene)
    {
        if (plaene is null) return 0;
        var list = plaene.ToList();
        var imported = 0;

        await _db.RunInTransactionAsync(conn =>
        {
            foreach (var p in list)
            {
                // IDs resetten
                p.Trainingsplan_Id = 0;
                foreach (var u in p.Uebungen ?? Enumerable.Empty<Uebung>())
                {
                    u.Uebung_Id = 0;
                    u.Trainingsplan_Id = 0;
                    u.TagId = 0;
                }

                // Plan anlegen
                conn.Insert(p);

                // Default-Tag anlegen (immer – konsistent fürs Import-Szenario)
                var defaultTag = new Tag
                {
                    Name = string.Empty,
                    Reihenfolge = 1,
                    Trainingsplan_Id = p.Trainingsplan_Id
                };
                conn.Insert(defaultTag);

                // Übungen dem Plan + Default-Tag zuordnen
                foreach (var u in p.Uebungen ?? Enumerable.Empty<Uebung>())
                {
                    u.Trainingsplan_Id = p.Trainingsplan_Id;
                    u.TagId = defaultTag.TagId;
                    conn.Insert(u);
                }

                imported++;
            }
        });

        return imported;
    }

    public async Task LoescheTrainingsplanKomplettAsync(int planId)
    {
        await _db.RunInTransactionAsync(conn =>
        {
            // 1) Sätze zu Übungen des Plans löschen
            var uebungen = conn.Table<Uebung>()
                               .Where(u => u.Trainingsplan_Id == planId)
                               .ToList();
            foreach (var u in uebungen)
            {
                conn.Execute("DELETE FROM Satz WHERE Uebung_Id = ?", u.Uebung_Id);
            }

            // 2) Übungen löschen
            foreach (var u in uebungen)
                conn.Delete(u);

            // 3) Tags löschen
            var tags = conn.Table<Tag>()
                           .Where(t => t.Trainingsplan_Id == planId)
                           .ToList();
            foreach (var t in tags)
                conn.Delete(t);

            // 4) Plan löschen
            var plan = conn.Find<Trainingsplan>(planId);
            if (plan is not null)
                conn.Delete(plan);
        });
    }


    public async Task SpeichereSatzAsync(Satz satz)
    {
        // Annahme: Uebung_Id, Jahr, KalenderWoche sind gesetzt (beim Anlegen via AddSatzAsync)
        if (satz.Satz_Id == 0)
            await _db.InsertAsync(satz);
        else
            await _db.UpdateAsync(satz);
    }

    public async Task LoescheSatzAsync(int satzId) // optional
    {
        var satz = await _db.FindAsync<Satz>(satzId);
        if (satz != null)
            await _db.DeleteAsync(satz);
    }




}
