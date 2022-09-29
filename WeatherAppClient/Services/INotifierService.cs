using WeatherApp.Shared.Entities;

namespace WeatherApp.Client.Services
{
    public interface INotifierService
    {
        event Func<string, Sensor, Task>? Notify;
        Task RefreshDailyStats(string key, Sensor machine);
    }
}