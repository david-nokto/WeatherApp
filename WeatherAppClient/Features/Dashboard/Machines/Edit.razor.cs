using Microsoft.AspNetCore.Components;
using WeatherApp.Client.Services;
using WeatherApp.Shared.Entities;

namespace WeatherAppClient.Features.Dashboard.Machines
{
    public partial class Edit : IDisposable
    {
        [Inject]
        public ISensorDataService SensorDataService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        INotifierService Notifier { get; set; } = default!;

        [Parameter]
        public string SensorId { get; set; } = string.Empty;

        public Sensor Sensor { get; set; } = new Sensor();
        public List<SensorType> SensorTypes { get; set; } = new List<SensorType>();
        public List<SensorLocation> Locations { get; set; } = new List<SensorLocation>();

        protected string Message { get; set; } = string.Empty;
        protected string StatusClass { get; set; } = string.Empty;
        protected bool Saved { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Saved = false;
            Notifier.Notify += OnNotify;
            SensorTypes = (await SensorDataService.GetAllSensorTypes()).ToList();
            Locations = (await SensorDataService.GetAllSensorLocations()).ToList();

            Guid.TryParse(SensorId, out var sensorId);

            if (sensorId == Guid.Empty)
            {
                Sensor = new Sensor
                {
                    Status = SensorStatus.Offline,
                    SensorTypeId = SensorTypes.Count > 0 ? SensorTypes.First().Id : default!,
                    LocationId = Locations.Count > 0 ? Locations.First().Id : default!
                };
            }
            else
            {
                Sensor = await SensorDataService.GetSensorDetails(sensorId) ?? new Sensor();
            }
        }

        public async Task OnNotify(string key, Sensor sensor)
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

        protected async Task HandleValidSubmit()
        {
            Saved = false;

            if (Sensor.Id == Guid.Empty)
            {
                Sensor.SensorType = SensorTypes.First(t => t.Id == Sensor.SensorTypeId);
                Sensor.Location = Locations.First(t => t.Id == Sensor.LocationId);
                var addedMachine = await SensorDataService.AddSensor(Sensor);
                if (addedMachine != null)
                {
                    StatusClass = "alert-success";
                    Message = "New machine added successfully!";
                    Saved = true;
                    await Notifier.RefreshDailyStats("add", Sensor);
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong adding the new machine, please try again.";
                    Saved = false;
                }
            }
            else
            {
                Sensor.SensorType = SensorTypes.First(t => t.Id == Sensor.SensorTypeId);
                Sensor.Location = Locations.First(t => t.Id == Sensor.LocationId);
                await SensorDataService.UpdateSensor(Sensor);
                StatusClass = "alert-success";
                Message = "Machine updated successfully!";
                Saved = true;
            }

        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors, please try again.";
        }

        protected void NavigateToDashboard()
        {
            NavigationManager.NavigateTo("/");
        }
    }
}