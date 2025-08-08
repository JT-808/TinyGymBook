using System.Collections.Generic;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.Trainingsplanservice;

public interface ITrainingsplanService
{
    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();
    Task SpeichereTrainingsplanAsync(Trainingsplan plan, IEnumerable<Tag> tage);
    Task LoescheTrainingsplanAsync(Trainingsplan plan);
    Task<List<Uebung>> LadeUebungenZuPlanAsync(int trainingsplanId);

    Task SpeichereUebung(Uebung uebung);
    Task SpeichereTagAsync(Tag tag);

    Task InitAsync();

    Task<List<Tag>> LadeTageAsync(int trainingsplanId);



    // nach umstruktirierung

    Task<List<Satz>> LadeSaetzeFuerUebungAsync(int uebungId);
    Task SpeichereSaetzeFuerUebungAsync(int uebungId, IEnumerable<Satz> saetze);






    // Optional: Nur im Json-Service sinnvoll implementiert
    //Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene);
}
