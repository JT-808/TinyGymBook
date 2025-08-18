using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Tiny_GymBook.Models;
using Windows.Storage; // für StorageFolder/StorageFile

namespace Tiny_GymBook.Services.TrainingsplanIO;

public class JsonTrainingsplanService : ITrainingsplanIOService
{
    private readonly string _exportOrdner;
    private readonly JsonSerializerOptions _jsonOptionen = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonTrainingsplanService()
    {
        var basis = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Tiny_GymBook");

        Directory.CreateDirectory(basis);
        _exportOrdner = Path.Combine(basis, "Exports");
        Directory.CreateDirectory(_exportOrdner);
    }

    public async Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync()
    {
        if (!Directory.Exists(_exportOrdner))
            return new List<Trainingsplan>();

        var result = new List<Trainingsplan>();

        foreach (var file in Directory.EnumerateFiles(_exportOrdner, "*.json"))
        {
            try
            {
                var jsonText = await File.ReadAllTextAsync(file);

                var list = JsonSerializer.Deserialize<List<Trainingsplan>>(jsonText, _jsonOptionen);
                if (list is { Count: > 0 })
                {
                    result.AddRange(list);
                    continue;
                }

                var single = JsonSerializer.Deserialize<Trainingsplan>(jsonText, _jsonOptionen);
                if (single is not null)
                    result.Add(single);
            }
            catch
            {
                // Datei überspringen
            }
        }

        return result;
    }

    public async Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync(Stream stream)
    {
        try
        {
            if (stream.CanSeek) stream.Position = 0;
            var list = await JsonSerializer.DeserializeAsync<List<Trainingsplan>>(stream, _jsonOptionen);
            if (list is { Count: > 0 }) return list;
        }
        catch (JsonException) { /* fallback */ }

        try
        {
            if (stream.CanSeek) stream.Position = 0;
            var single = await JsonSerializer.DeserializeAsync<Trainingsplan>(stream, _jsonOptionen);
            if (single is not null) return new[] { single };
        }
        catch (JsonException) { /* ignore */ }

        return Array.Empty<Trainingsplan>();
    }

    // Standard-Export in Service-Ordner
    public async Task SpeichereTrainingsplanAsync(Trainingsplan plan)
    {
        var zielDatei = Path.Combine(_exportOrdner, BuildFileName(plan));
        await File.WriteAllTextAsync(zielDatei, Serialize(plan));
    }

    // ===== NEU: Helfer & Export in gewählten Ordner =====

    public string BuildFileName(Trainingsplan plan) => $"{Sanitize(plan.Name)}.json";

    public string Serialize(Trainingsplan plan) => JsonSerializer.Serialize(plan, _jsonOptionen);

    public async Task<StorageFile> SaveToFolderAsync(StorageFolder folder, Trainingsplan plan, bool overwrite = true)
    {
        var file = await folder.CreateFileAsync(
            BuildFileName(plan),
            overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.FailIfExists);

        await FileIO.WriteTextAsync(file, Serialize(plan));
        return file;
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim('_');
    }
}
