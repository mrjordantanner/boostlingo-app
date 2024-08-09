using Boostlingo.Models;
using Boostlingo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Boostlingo
{
    public class DataProcessor
    {
        private readonly IConfiguration _configuration;
        private readonly IJsonDataService _jsonDataService;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DataProcessor> _logger;

        public DataProcessor(
            IConfiguration configuration,
            IJsonDataService jsonDataService,
            IDatabaseService databaseService,
            ILogger<DataProcessor> logger)
        {
            _configuration = configuration;
            _jsonDataService = jsonDataService;
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task ProcessData()
        {
            _logger.LogInformation("Starting data processing.");

            var dataUrl = GetConfigProperty("AppSettings:DataUrl");
            var tableName = GetConfigProperty("Postgres:TableName");

            if (string.IsNullOrEmpty(dataUrl) || string.IsNullOrEmpty(tableName)) return;

            string response;
            try
            {
                response = await _jsonDataService.GetJsonDataAsync(dataUrl);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogError("Response from JsonDataService was null or empty.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching JSON data: {ex}", ex);
                return;
            }

            // Deserialize json
            var models = JsonConvert.DeserializeObject<List<DummyModel>>(response);
            if (models == null || models.Count == 0)
            {
                _logger.LogError("Error deserializing JSON data.");
                return;
            }

            var processedModels = SplitName(models);

            // Clear table (for testing purposes), then Write Data
            await _databaseService.ClearTableAsync(tableName);
            await _databaseService.WriteDummyDataAsync(processedModels);

            // Read from table
            var dbResponse = await _databaseService.ReadDummyDataAsync();
            if (dbResponse == null || dbResponse.Count == 0)
            {
                _logger.LogError("Error reading data from table: {tableName}.", tableName);
                return;
            }

            // Sort and print data
            dbResponse.OrderBy(model => model.LastName)
                         .ThenBy(model => model.FirstName)
                         .ToList()
                         .ForEach(model => Console.WriteLine($"{model.LastName}, {model.FirstName}"));

            _logger.LogInformation("Operation completed successfully. Application will now exit.");
        }

        // Helper methods
        private static List<DummyModel> SplitName(List<DummyModel> models)
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

        private string GetConfigProperty(string propertyString)
        {
            var config = _configuration[propertyString];
            if (string.IsNullOrEmpty(config))
            {
                var errorMessage = $"Configuration error: {propertyString} not found.";

                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
            return config;
        }


    }
}
