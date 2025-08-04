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

    public async Task InitAsync()
    {
        // Tabellen erzeugen, falls sie noch nicht existieren
        await _db.CreateTableAsync<Trainingsplan>();
        await _db.CreateTableAsync<Uebung>();
        await _db.CreateTableAsync<Trainingseintrag>();
        await _db.CreateTableAsync<Satz>();
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

    public async Task SpeichereTrainingsplanAsync(Trainingsplan plan)
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

        if (plan.Uebungen != null)
        {
            foreach (var uebung in plan.Uebungen)
            {
                uebung.Trainingsplan_Id = plan.Trainingsplan_Id;
                if (uebung.Uebung_Id == 0)
                {
                    await _db.InsertAsync(uebung);
                    Debug.WriteLine($"[DEBUG] Übung '{uebung.Name}' gespeichert mit Id={uebung.Uebung_Id} für Plan {plan.Name} ({plan.Trainingsplan_Id})");
                }
                else
                {
                    await _db.UpdateAsync(uebung);
                    Debug.WriteLine($"[DEBUG] Übung '{uebung.Name}' aktualisiert, Id={uebung.Uebung_Id}, PlanId={uebung.Trainingsplan_Id}");
                }
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


    public async Task<bool> UebungWirklichGespeichertUndZugeordnet(Uebung uebung)
    {
        var gespeicherte = await _db.Table<Uebung>().Where(u => u.Uebung_Id == uebung.Uebung_Id).FirstOrDefaultAsync();
        if (gespeicherte == null)
            return false;
        return gespeicherte.Trainingsplan_Id == uebung.Trainingsplan_Id;
    }

}
