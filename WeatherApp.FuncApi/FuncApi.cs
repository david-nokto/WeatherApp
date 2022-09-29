using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WeatherApp.FuncApi.Models;
using WeatherApp.FuncApi.Services;
using WeatherApp.Shared.Entities;
using CloudTable = Microsoft.Azure.Cosmos.Table.CloudTable;
using TableAttribute = Microsoft.Azure.WebJobs.TableAttribute;
using TableOperation = Microsoft.Azure.Cosmos.Table.TableOperation;

namespace WeatherApp.FuncApi
{
    public class FuncApi
    {
        private readonly IWeatherDataService smhi;

        public FuncApi(IWeatherDataService smhi)
        {
            this.smhi = smhi;
        }

        [FunctionName("Create")]
        public static async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sensors")] HttpRequest req,
            [Table("sensors", Connection = "AzureWebJobsStorage")] IAsyncCollector<SensorTableEntity> sensorTable,
            ILogger log)
        {
            log.LogInformation("Add new sensor to storage");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var sensor = JsonConvert.DeserializeObject<Sensor>(requestBody);
            sensor.Id = sensor.Id == Guid.Empty ? Guid.NewGuid() : sensor.Id;

            if (sensor is null) return new BadRequestResult();

            await sensorTable.AddAsync(sensor.ToTableEntity());

            return new OkObjectResult(sensor);
        }

        [FunctionName("Get")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sensors")] HttpRequest req,
            [Table("sensors", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log)
        {
            log.LogInformation("Get all sensors");

            var query = new TableQuery<SensorTableEntity>();
            var res = await table.ExecuteQuerySegmentedAsync(query, null);

            var response = res.Select(Mapper.ToSensor).ToList();
            foreach (var sensor in response)
            {
                sensor.SensorData = await smhi.GetPointForeCast(sensor.Location.Longitude, sensor.Location.Latitude);
            }

            return new OkObjectResult(response);
        }

        [FunctionName("GetTypes")]
        public static async Task<IActionResult> GetTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sensortypes")] HttpRequest req,
            [Table("sensors", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log)
        {
            log.LogInformation("Get all sensor types");

            var query = new TableQuery<SensorTableEntity>();
            var res = await table.ExecuteQuerySegmentedAsync(query, null);

            var response = res.Select(Mapper.ToSensor).Select(m => m.SensorType).Distinct().ToList();

            if (!response.Exists(t => t.Name == "Temperature Sensor"))
            {
                response.Add(new SensorType
                {
                    Id = Guid.NewGuid(),
                    Name = "Temperature Sensor",
                    Description = "Temperature in Celsius"
                });
            }

            return new OkObjectResult(response);
        }

        [FunctionName("GetLocations")]
        public static async Task<IActionResult> GetLocations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sensorlocations")] HttpRequest req,
            [Table("sensors", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log)
        {
            log.LogInformation("Get all sensor locations");

            var query = new TableQuery<SensorTableEntity>();
            var res = await table.ExecuteQuerySegmentedAsync(query, null);

            var response = res.Select(Mapper.ToSensor).Select(m => m.Location).Distinct().ToList();

            if (!response.Exists(l => l.City == "Stockholm"))
            {
                response.Add(new SensorLocation
                {
                    Id = Guid.NewGuid(),
                    Country = "Sweden",
                    City = "Stockholm",
                    Longitude = 18.069f,
                    Latitude = 59.324f
                });
            }
            if (!response.Exists(l => l.City == "Göteborg"))
            {
                response.Add(new SensorLocation
                {
                    Id = Guid.NewGuid(),
                    Country = "Sweden",
                    City = "Göteborg",
                    Longitude = 11.975f,
                    Latitude = 57.709f
                });
            }
            if (!response.Exists(l => l.City == "Luleå"))
            {
                response.Add(new SensorLocation
                {
                    Id = Guid.NewGuid(),
                    Country = "Sweden",
                    City = "Luleå",
                    Longitude = 22.157f,
                    Latitude = 65.585f
                });
            }

            return new OkObjectResult(response);
        }

        [FunctionName("GetDetails")]
        public async Task<IActionResult> GetDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sensors/{id}")] HttpRequest req,
            [Table("sensors", "Sensor", "{id}", Connection = "AzureWebJobsStorage")] SensorTableEntity sensorTableToReturn,
            string id,
            ILogger log)
        {
            log.LogInformation("Get details for sensor");

            if (sensorTableToReturn is null || sensorTableToReturn.RowKey != id) return new NotFoundResult();

            var response = sensorTableToReturn.ToSensor();
            var forecast = await smhi.GetPointForeCast(response.Location.Longitude, response.Location.Latitude);
            response.SensorData = forecast;

            return new OkObjectResult(response);
        }

        [FunctionName("GetNewData")]
        public async Task<IActionResult> GetNewData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sensordata/{id}/new")] HttpRequest req,
            [Table("sensors", "Sensor", "{id}", Connection = "AzureWebJobsStorage")] SensorTableEntity sensorTableEntity,
            string id,
            ILogger log)
        {
            log.LogInformation("Get new data for sensor");

            if (sensorTableEntity is null || sensorTableEntity.RowKey != id) return new NotFoundResult();

            var sensor = sensorTableEntity.ToSensor();
            var forecast = await smhi.GetPointForeCast(sensor.Location.Longitude, sensor.Location.Latitude);

            return new OkObjectResult(forecast);
        }

        [FunctionName("Put")]
        public static async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "sensors/{id}")] HttpRequest req,
            [Table("sensors", Connection = "AzureWebJobsStorage")] CloudTable table,
            Guid id,
            ILogger log)
        {
            log.LogInformation("Update sensor");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var sensorToUpdate = JsonConvert.DeserializeObject<Sensor>(requestBody);

            if (sensorToUpdate is null || sensorToUpdate.Id != id) return new BadRequestResult();

            var sensorEntity = sensorToUpdate.ToTableEntity();
            sensorEntity.ETag = "*";

            var operation = TableOperation.Replace(sensorEntity);
            await table.ExecuteAsync(operation);

            return new NoContentResult();
        }

        [FunctionName("Delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "sensors/{id}")] HttpRequest req,
            [Table("sensors", "Sensor", "{id}", Connection = "AzureWebJobsStorage")] SensorTableEntity sensorTableToDelete,
            [Table("sensors", Connection = "AzureWebJobsStorage")] CloudTable table,
            [Queue("deletequeue", Connection = "AzureWebJobsStorage")] IAsyncCollector<SensorTableEntity> queue,
            ILogger log)
        {
            log.LogInformation("Delete sensor");

            if (sensorTableToDelete is null) return new BadRequestResult();

            var operation = TableOperation.Delete(sensorTableToDelete);
            await table.ExecuteAsync(operation);

            // add to queue
            await queue.AddAsync(sensorTableToDelete);

            return new NoContentResult();
        }

        [FunctionName("GetRemovedFromQueue")]
        public static async Task GetRemovedFromQueue(
          [QueueTrigger("deletequeue", Connection = "AzureWebJobsStorage")] SensorTableEntity item,
          [Blob("removed", Connection = "AzureWebJobsStorage")] CloudBlobContainer blobContainer,
          ILogger log)
        {
            log.LogInformation("Queue trigger started...");

            await blobContainer.CreateIfNotExistsAsync();
            var blob = blobContainer.GetBlockBlobReference($"{item.RowKey}.txt");
            await blob.UploadTextAsync($"{DateTime.Now}: {item.Name} was removed");
        }
    }
}
