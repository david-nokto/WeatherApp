using Microsoft.Azure.Cosmos.Table;
using WeatherApp.Shared.Entities;

namespace WeatherApp.FuncApi.Models
{
    public class SensorTableEntity : TableEntity
    {
        public string Name { get; set; }
        public int Status { get; set; } = (int)SensorStatus.Offline;
        public string Id { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
    }
}
