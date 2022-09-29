using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Shared.Entities
{
    public class SensorData
    {
        public Guid Id { get; set; }

        [Required]
        public double Data { get; set; }

        [Required]
        public DateTime Time { get; set; }

        public Guid MachineId { get; set; }
    }
}