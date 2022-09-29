using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Shared.Entities
{
    public class Sensor
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public SensorStatus Status { get; set; }

        public SensorLocation Location { get; set; } = new SensorLocation();
        public Guid LocationId { get; set; }

        public SensorType SensorType { get; set; } = new SensorType();
        public Guid SensorTypeId { get; set; }

        public ICollection<SensorData> SensorData { get; set; } = new List<SensorData>();
    }
}
