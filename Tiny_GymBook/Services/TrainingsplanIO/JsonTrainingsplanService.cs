using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

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

    // Lädt alle Pläne aus allen *.json-Dateien im Export-Ordner
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

                // Zuerst versuchen: Liste von Plänen
                var list = JsonSerializer.Deserialize<List<Trainingsplan>>(jsonText, _jsonOptionen);
                if (list is { Count: > 0 })
                {
                    result.AddRange(list);
                    continue;
                }

                // Fallback: einzelner Plan
                var single = JsonSerializer.Deserialize<Trainingsplan>(jsonText, _jsonOptionen);
                if (single is not null)
                    result.Add(single);
            }
            catch
            {
                // Datei überspringen, wenn sie nicht lesbar ist
            }
        }

        return result;
    }

    // Schreibt ALLE Pläne in eine Sammeldatei im Export-Ordner
    public async Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene)
    {
        var zielDatei = Path.Combine(_exportOrdner, "trainingsplaene.json");
        var json = JsonSerializer.Serialize(plaene, _jsonOptionen);
        await File.WriteAllTextAsync(zielDatei, json);
    }


    public async Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync(Stream stream)
    {
        // 1) Versuche: Liste von Plänen
        try
        {
            if (stream.CanSeek) stream.Position = 0;
            var list = await JsonSerializer.DeserializeAsync<List<Trainingsplan>>(stream, _jsonOptionen);
            if (list is { Count: > 0 })
                return list;
        }
        catch (JsonException)
        {
            // ignorieren, gleich Einzelobjekt versuchen
        }

        // 2) Fallback: einzelner Plan
        try
        {
            if (stream.CanSeek) stream.Position = 0;
            var single = await JsonSerializer.DeserializeAsync<Trainingsplan>(stream, _jsonOptionen);
            if (single is not null)
                return new[] { single };
        }
        catch (JsonException)
        {
            // ignorieren
        }

        return Array.Empty<Trainingsplan>();
    }

    // Schreibt EINEN Plan in eine eigene Datei (Planname.json)
    public async Task SpeichereTrainingsplanAsync(Trainingsplan plan)
    {
        var dateiName = $"{Sanitize(plan.Name)}.json";
        var zielDatei = Path.Combine(_exportOrdner, dateiName);

        // Einzelobjekt speichern (keine Liste mit 1 Element)
        var json = JsonSerializer.Serialize(plan, _jsonOptionen);
        await File.WriteAllTextAsync(zielDatei, json);
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim('_');
    }
}
