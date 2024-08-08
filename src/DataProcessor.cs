using Boostlingo.Models;
using Boostlingo.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Boostlingo
{
    public class DataProcessor
    {
        private readonly IConfiguration _configuration;
        private readonly IJsonDataService _jsonDataService;
        private readonly IDatabaseService _databaseService;

        public DataProcessor(
            IConfiguration configuration, 
            IJsonDataService jsonDataService, 
            IDatabaseService databaseService)
        {
            _configuration = configuration;
            _jsonDataService = jsonDataService;
            _databaseService = databaseService;
        }

        public async Task ProcessData()
        {
            Console.WriteLine("Starting data processing.");

            var jsonDataUrl = _configuration["AppSettings:JsonDataUrl"];
            var tableName = _configuration["Postgres:TableName"];

            string response;
            try
            {
                response = await _jsonDataService.GetJsonDataAsync(jsonDataUrl);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching JSON data: {ex}");
            }
            if (string.IsNullOrEmpty(response))
            {
                throw new Exception("Response was null or empty.");
            }

            // Deserialize json into List of DummyModels
            var models = JsonConvert.DeserializeObject<List<DummyModel>>(response);
            if (models == null || models.Count == 0)
            {
                throw new Exception("Error deserializing JSON data.");
            }

            var processedModels = SplitNames(models);

            // Clear table (for testing purposes), then Write Data
            await _databaseService.ClearTableAsync(tableName);
            await _databaseService.WriteDummyDataAsync(processedModels);

            // Read from table
            var dbResponse = await _databaseService.ReadDummyDataAsync();
            if (dbResponse == null || dbResponse.Count == 0)
            {
                throw new Exception($"Error reading data from {tableName} table.");
            }

            // Sort data by Last and then First name and output to console
            var sortedModels = dbResponse.OrderBy(model => model.LastName)
                         .ThenBy(model => model.FirstName)
                         .ToList();

            sortedModels.ForEach(model => Console.WriteLine($"{model.LastName}, {model.FirstName}"));
        }

        private static List<DummyModel> SplitNames(List<DummyModel> models)
        {
            foreach (var model in models)
            {
                var names = model.Name.Split(' ');
                if (names.Length > 1)
                {
                    model.FirstName = names[0];
                    model.LastName = names[1];
                }
            }
            return models;
        }


    }
}
