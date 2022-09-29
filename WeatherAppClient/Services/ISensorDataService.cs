using WeatherApp.Shared.Entities;

namespace WeatherApp.Client.Services
{
    public interface ISensorDataService
    {
        Task<IEnumerable<Sensor>> GetAllSensors();
        Task<Sensor?> GetSensorDetails(Guid sensorId);
        Task<Sensor?> AddSensor(Sensor sensor);
        Task UpdateSensor(Sensor sensor);
        Task DeleteSensor(Guid sensorId);
        Task<IEnumerable<SensorType>> GetAllSensorTypes();
        Task<IEnumerable<SensorLocation>> GetAllSensorLocations();
        Task<IEnumerable<SensorData>> GetNewSensorData(Guid sensorId);
    }
}
