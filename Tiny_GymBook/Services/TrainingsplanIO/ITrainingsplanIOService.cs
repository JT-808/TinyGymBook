using System.Collections.Generic;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.TrainingsplanIO;

public interface ITrainingsplanIOService
{

    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();

    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync(Stream stream);


    public Task SpeichereAlleTrainingsplaeneJsonAsync(IEnumerable<Trainingsplan> plaene);

    public Task SpeichereTrainingsplanAsync(Trainingsplan plan); //einzelner Plan



}
