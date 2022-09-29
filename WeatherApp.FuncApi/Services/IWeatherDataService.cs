using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApp.Shared.Entities;

namespace WeatherApp.FuncApi.Services
{
    public interface IWeatherDataService
    {
        Task<List<SensorData>> GetPointForeCast(float longitude, float latitude);
    }
}
