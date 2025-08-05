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
    Task SpeichereTagAsync(Tag tag);

    Task<bool> UebungWirklichGespeichertUndZugeordnet(Uebung uebung);


    Task InitAsync();


    Task SpeichereTrainingseintragAsync(Trainingseintrag eintrag);

    Task<List<Trainingseintrag>> LadeTrainingseintraegeAsync(int trainingsplanId);
    Task<List<Tag>> LadeTageAsync(int trainingsplanId);

    // Optional: Nur im Json-Service sinnvoll implementiert
    //Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene);
}
