// ITrainingsplanIOService.cs
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tiny_GymBook.Models;
using Windows.Storage; // Uno/WinRT-Typen sind auf Linux/Android verfügbar

public interface ITrainingsplanIOService
{
    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();
    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync(Stream stream);

    Task SpeichereTrainingsplanAsync(Trainingsplan plan); // Standard-Export in Service-Ordner

    // Ausgelagerte Helfer & Export in gewählten Ordner
    string BuildFileName(Trainingsplan plan);             // z.B. "Mein_Plan.json"
    string Serialize(Trainingsplan plan);                 // JSON mit einheitlichen Optionen
    Task<StorageFile> SaveToFolderAsync(StorageFolder folder, Trainingsplan plan, bool overwrite = true);
}
