// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Tiny_GymBook.Models;

// namespace Tiny_GymBook.Services.Trainingsplanservice
// {
//     public class JsonTrainingsplanService : ITrainingsplanService
//     {
//         private readonly string _dateipfad;
//         private readonly JsonSerializerOptions _jsonOptionen = new()
//         {
//             WriteIndented = true,
//             PropertyNameCaseInsensitive = true
//         };

//         public JsonTrainingsplanService()
//         {
//             var ordner = Path.Combine(
//                 Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
//                 "Tiny_GymBook"
//             );

//             Directory.CreateDirectory(ordner);
//             _dateipfad = Path.Combine(ordner, "trainingsplaene.json");
//         }

//         public async Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync()
//         {
//             if (!File.Exists(_dateipfad))
//             {
//                 // Beispielinitialisierung
//                 var initial = new List<Trainingsplan>
//                 {
//                     new("Initialer Plan", new List<Uebung>
//                     {
//                         new("Bankdrücken", Muskelgruppe.Brust),
//                         new("Kniebeuge", Muskelgruppe.Beine)
//                     })
//                 };

//                 await SpeichereAlleTrainingsplaeneJsonAsync(initial);
//                 return initial;
//             }

//             try
//             {
//                 var jsonText = await File.ReadAllTextAsync(_dateipfad);
//                 var plaene = JsonSerializer.Deserialize<List<Trainingsplan>>(jsonText, _jsonOptionen);
//                 return plaene ?? new List<Trainingsplan>();
//             }
//             catch
//             {
//                 // Fehler beim Parsen – Rückfall auf leere Liste
//                 return new List<Trainingsplan>();
//             }
//         }

//         public Task LoescheTrainingsplanAsync(Trainingsplan plan)
//         {
//             throw new NotImplementedException();
//         }

//         public async Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene)
//         {
//             var json = JsonSerializer.Serialize(plaene, _jsonOptionen);
//             await File.WriteAllTextAsync(_dateipfad, json);
//         }

//         public Task SpeichereTrainingsplanAsync(Trainingsplan plan)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }
