using System.Collections.Generic;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.DataService;

public interface IDataService
{
    Task InitAsync();
    Task SpeichereTrainingsplanAsync(Trainingsplan plan, IEnumerable<Tag> tage);
    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();
    Task LoescheTrainingsplanAsync(Trainingsplan plan);
    Task SpeichereUebung(Uebung uebung);
    Task<List<Uebung>> LadeUebungenZuPlanAsync(int trainingsplanId);
    Task SpeichereTagAsync(Tag tag);
    Task<List<Tag>> LadeTageAsync(int trainingsplanId);


    //TODO Löschetag und LöscheÜbung


    Task<List<Satz>> LadeSaetzeFuerUebungAsync(int uebungId);
    Task SpeichereSaetzeFuerUebungAsync(int uebungId, IEnumerable<Satz> saetze);


}
