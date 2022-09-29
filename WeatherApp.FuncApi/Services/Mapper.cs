using System;
using System.Collections.Generic;
using WeatherApp.FuncApi.Models;
using WeatherApp.Shared.Entities;

namespace WeatherApp.FuncApi.Services
{
    public static class Mapper
    {
        public static SensorTableEntity ToTableEntity(this Sensor sensor)
        {
            return new SensorTableEntity
            {
                Name = sensor.Name,
                Status = (int)sensor.Status,
                Id = sensor.LocationId.ToString(),
                City = sensor.Location.City,
                Country = sensor.Location.Country,
                Longitude = sensor.Location.Longitude.ToString(),
                Latitude = sensor.Location.Latitude.ToString(),
                TypeId = sensor.SensorTypeId.ToString(),
                TypeDescription = sensor.SensorType.Description,
                TypeName = sensor.SensorType.Name,
                PartitionKey = "sensor",
                RowKey = sensor.Id.ToString()
            };
        }

        public static Sensor ToSensor(this SensorTableEntity sensorTable)
        {
            return new Sensor
            {
                Id = Guid.Parse(sensorTable.RowKey),
                Name = sensorTable.Name,
                Status = (SensorStatus)sensorTable.Status,
                LocationId = Guid.Parse(sensorTable.Id),
                Location = new SensorLocation
                {
                    Id = Guid.Parse(sensorTable.Id),
                    City = sensorTable.City,
                    Country = sensorTable.Country,
                    Longitude = float.Parse(sensorTable.Longitude),
                    Latitude = float.Parse(sensorTable.Latitude)
                },
                SensorTypeId = Guid.Parse(sensorTable.TypeId),
                SensorType = new SensorType
                {
                    Id = Guid.Parse(sensorTable.TypeId),
                    Name = sensorTable.TypeName,
                    Description = sensorTable.TypeDescription
                },
                SensorData = new List<SensorData>()
            };
        }
    }
}
