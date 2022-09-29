using WeatherApp.Shared.Entities;

namespace WeatherApp.Client.Services
{
    public class NotifierService : INotifierService
    {

        public async Task RefreshDailyStats(string key, Sensor machine)
        {
            if (Notify != null)
            {
                await Notify.Invoke(key, machine);
            }
        }

        public event Func<string, Sensor, Task>? Notify;
    }
}
