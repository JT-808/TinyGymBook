using SQLite;
using System;
using System.Collections.Generic;
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
        _ = InitAsync();
    }

    private async Task InitAsync()
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
            plan.Uebungen = alleUebungen
                .Where(u => u.Trainingsplan_Id == plan.Trainingsplan_Id)
                .ToList();
        }
        return plaene;
    }

    public async Task SpeichereTrainingsplanAsync(Trainingsplan plan)
    {
        // 1. Plan speichern

        await _db.InsertAsync(plan);



        //    2. JETZT die Plan-ID an allen Übungen setzen!
        // if (plan.Uebungen != null)
        // {
        //     foreach (var uebung in plan.Uebungen)
        //     {
        //         uebung.Trainingsplan_Id = plan.Trainingsplan_Id;
        //         if (uebung.Uebung_Id == 0)
        //             await _db.InsertAsync(uebung);
        //         else
        //             await _db.UpdateAsync(uebung);
        //     }


        // if (plan.Uebungen != null)
        // {
        //     foreach (var uebung in plan.Uebungen)
        //     {
        //         uebung.Trainingsplan_Id = plan.Trainingsplan_Id;

        //         await _db.InsertAsync(uebung);
        //     }
        // }



    }


    public async Task SpeichereUebung(Uebung uebung)
    {
        await _db.InsertAsync(uebung);
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



    // Für Kompatibilität mit Interface (JSON-Export kannst du später ausbauen)
    // public Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene)
    // {
    //     throw new NotImplementedException("JSON-Export nicht implementiert (nur für Testzwecke).");
    // }




    // === Erweiterbar ===
    // Hier kannst du analog weitere Methoden bauen, etwa:
    // - TrainingsplanByIdAsync(int id)
    // - Übungen/Einträge/Sätze laden & speichern
}
