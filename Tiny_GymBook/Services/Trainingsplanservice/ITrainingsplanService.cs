using System.Collections.Generic;
using System.Threading.Tasks;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Services.Trainingsplanservice
{
    public interface ITrainingsplanService
    {
        Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();


        //Task SpeichereTrainingsplanAsync(Trainingsplan plan);

        Task SpeichereAlleTrainingsplaeneAsync(IEnumerable<Trainingsplan> plaene);
    }
}
