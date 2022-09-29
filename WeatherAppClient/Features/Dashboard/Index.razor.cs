using Microsoft.AspNetCore.Components;
using WeatherApp.Client.Services;
using WeatherApp.Shared.Entities;

namespace WeatherAppClient.Features.Dashboard
{
    public partial class Index : IDisposable
    {
        public List<Sensor> Sensors { get; set; } = new List<Sensor>();

        [Inject]
        public ISensorDataService SensorDataService { get; set; } = default!;

        [Inject]
        INotifierService Notifier { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Sensors = (await SensorDataService.GetAllSensors()).ToList();
            Notifier.Notify += OnNotify;
        }

        public async Task OnNotify(string key, Sensor machine)
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }

        private void ToggleSensor(object checkedValue, Sensor sensor)
        {
            sensor.Status = (bool)checkedValue ? SensorStatus.Online : SensorStatus.Offline;
            SensorDataService.UpdateSensor(sensor);
            Notifier.RefreshDailyStats("status", sensor);
        }

        private void DeleteSensor(Sensor sensor)
        {
            SensorDataService.DeleteSensor(sensor.Id);
            Sensors.Remove(sensor);
            Notifier.RefreshDailyStats("delete", sensor);
        }
    }
}