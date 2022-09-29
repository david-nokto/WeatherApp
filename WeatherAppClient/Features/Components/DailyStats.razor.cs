using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WeatherApp.Client.Services;
using WeatherApp.Shared.Entities;

namespace WeatherAppClient.Features.Components
{
    public partial class DailyStats : IDisposable
    {
        public List<Sensor> Sensors { get; set; } = new List<Sensor>();

        public int CountTotal { get; set; }
        public int CountOnline { get; set; }
        public int CountOffline { get; set; }
        public int NumberOfReadings { get; set; }

        public string RefreshHoverClass { get; set; } = string.Empty;

        public string Visible { get; set; } = "visible";

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISensorDataService SensorDataService { get; set; } = default!;

        [Inject]
        INotifierService Notifier { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await RetrieveData();
            Notifier.Notify += OnNotify;
        }

        public async Task OnNotify(string key, Sensor sensor)
        {
            await InvokeAsync(() =>
            {
                var localMachine = Sensors.FirstOrDefault(m => m.Id == sensor.Id);

                switch (key)
                {
                    case "delete":
                        Sensors.Remove(localMachine!);
                        break;
                    case "status":
                        localMachine!.Status = sensor.Status;
                        break;
                    case "add":
                        Sensors.Add(sensor);
                        break;
                }

                SetStats();
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }

        private async Task RetrieveData()
        {
            var sensors = await SensorDataService.GetAllSensors();
            Sensors = sensors != null ? sensors.ToList() : new List<Sensor>();
            SetStats();
        }

        private void SetStats()
        {
            CountTotal = Sensors.Count();
            CountOnline = Sensors.Where(m => m.Status == SensorStatus.Online).Count();
            CountOffline = CountTotal - CountOnline;
            NumberOfReadings = 0;

            foreach (Sensor m in Sensors)
            {
                DateTime now = DateTime.Now;
                NumberOfReadings += m.SensorData.Where(d => d.Time.Date == now.Date).Count();
            }
        }

        private async Task RefreshStats()
        {
            Visible = "invisible";
            Thread.Sleep(300); // To give a sense of a refresh animation
            await RetrieveData();
            Visible = "visible";
        }

        private void MouseOver(MouseEventArgs e) { RefreshHoverClass = "refresh-hover"; }
        private void MouseOut(MouseEventArgs e) { RefreshHoverClass = ""; }
    }
}