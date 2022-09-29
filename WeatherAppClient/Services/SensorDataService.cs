using System.Text;
using System.Text.Json;
using WeatherApp.Shared.Entities;

namespace WeatherApp.Client.Services
{
    public class SensorDataService : ISensorDataService
    {
        private readonly HttpClient _httpClient;

        public SensorDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Sensor?> AddSensor(Sensor sensor)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(sensor), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/sensors", jsonContent);

            Sensor? responseContent = null;

            if (response.IsSuccessStatusCode)
            {
                responseContent = await JsonSerializer.DeserializeAsync<Sensor>(await response.Content.ReadAsStreamAsync()) ??
                    throw new Exception("Received null from successful Post");
            }

            return responseContent;
        }

        public async Task DeleteSensor(Guid sensorId)
        {
            await _httpClient.DeleteAsync($"api/sensors/{sensorId}");
        }

        public async Task<IEnumerable<SensorLocation>> GetAllSensorLocations()
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<SensorLocation>>
                (await _httpClient.GetStreamAsync($"api/sensorlocations"),
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? new List<SensorLocation>();
        }

        public async Task<IEnumerable<Sensor>> GetAllSensors()
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<Sensor>>
                (await _httpClient.GetStreamAsync($"api/sensors"),
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? new List<Sensor>();
        }

        public async Task<IEnumerable<SensorType>> GetAllSensorTypes()
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<SensorType>>
                (await _httpClient.GetStreamAsync($"api/sensortypes"),
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? new List<SensorType>();
        }

        public async Task<Sensor?> GetSensorDetails(Guid sensorId)
        {
            return await JsonSerializer.DeserializeAsync<Sensor>
                (await _httpClient.GetStreamAsync($"api/sensors/{sensorId}"),
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        public async Task<IEnumerable<SensorData>> GetNewSensorData(Guid sensorId)
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<SensorData>>
                (await _httpClient.GetStreamAsync($"api/sensordata/{sensorId}/new"),
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? new List<SensorData>();
        }

        public async Task UpdateSensor(Sensor sensor)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(sensor), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"api/sensors/{sensor.Id}", jsonContent);
        }
    }
}
