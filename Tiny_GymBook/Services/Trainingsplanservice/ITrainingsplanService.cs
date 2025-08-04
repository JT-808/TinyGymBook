using System.Collections.Generic;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.Trainingsplanservice;

public interface ITrainingsplanService
{
    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();
    Task SpeichereTrainingsplanAsync(Trainingsplan plan);
    Task LoescheTrainingsplanAsync(Trainingsplan plan);
    Task<List<Uebung>> LadeUebungenZuPlanAsync(int trainingsplanId);

    Task SpeichereUebung(Uebung uebung);

    Task<bool> UebungWirklichGespeichertUndZugeordnet(Uebung uebung);


    Task InitAsync();

    // Optional: Nur im Json-Service sinnvoll implementiert
    //Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene);
}
