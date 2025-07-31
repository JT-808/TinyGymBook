using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.Trainingsplanservice
{
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
            // TODO: Evtl. auch zugehörige Übungen & Einträge mitladen (JOIN, falls nötig)
            return await _db.Table<Trainingsplan>().ToListAsync();
        }

        public async Task SpeichereTrainingsplanAsync(Trainingsplan plan)
        {
            var result = await _db.InsertAsync(plan);
            Debug.WriteLine($"[DEBUG] InsertAsync Ergebnis: {result}");
            if (result > 0)
                Debug.WriteLine($"[DEBUG] Trainingsplan gespeichert! Neue ID: {plan.Trainingsplan_Id}");
            else
                Debug.WriteLine("[WARN] Trainingsplan NICHT gespeichert!");
        }

        public async Task LoescheTrainingsplanAsync(Trainingsplan plan)
        {
            await _db.DeleteAsync(plan);
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
}
