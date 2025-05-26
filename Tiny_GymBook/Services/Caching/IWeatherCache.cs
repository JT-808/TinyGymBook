namespace Tiny_GymBook.Services.Caching;
using WeatherForecast = Tiny_GymBook.Client.Models.WeatherForecast;
public interface IWeatherCache
{
    ValueTask<IImmutableList<WeatherForecast>> GetForecast(CancellationToken token);
}
