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

        private readonly string _dataUrl;

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

            _dataUrl = _configuration["AppSettings:DataUrl"];
            if (string.IsNullOrEmpty(_dataUrl))
            {
                _logger.LogError("Configuration error: DataUrl not found.");
            }
        }

        public async Task<bool> ProcessData()
        {
            _logger.LogInformation("Starting data processing.");

            var models = await GetDummyData();
            if (models == null) return false;

            var processedModels = SplitName(models);

            // Clear table (for testing purposes), then Write Data
            await _databaseService.ClearTableAsync();
            bool writeSuccess = await _databaseService.WriteDummyDataAsync(processedModels);
            if (!writeSuccess) return false;

            // Read from table
            var dbResponse = await _databaseService.ReadDummyDataAsync();
            if (dbResponse == null || dbResponse.Count == 0)
            {
                _logger.LogError("Error reading data from table.");
                return false;
            }

            // Sort and print data
            var sortedModels = dbResponse
                .OrderBy(model => model.LastName)
                .ThenBy(model => model.FirstName)
                .ToList();

            PrintModels(sortedModels);

            _logger.LogInformation("Operation completed successfully. Application will now exit.");
            return true;
        }

        private async Task<List<DummyModel>> GetDummyData()
        {
            string response;
            try
            {
                if (string.IsNullOrEmpty(_dataUrl)) return null;

                response = await _jsonDataService.GetJsonDataAsync(_dataUrl);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogError("Response from JsonDataService was null or empty.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching JSON data: {ex}", ex);
                return null;
            }

            // Deserialize json
            var models = JsonConvert.DeserializeObject<List<DummyModel>>(response);
            if (models == null || models.Count == 0)
            {
                _logger.LogError("Error deserializing JSON data.");
                return null;
            }

            return models;
        }

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

        private static void PrintModels(List<DummyModel> sortedModels)
        {
            foreach (var model in sortedModels)
            {
                Console.WriteLine(model.Id);
                Console.WriteLine($"{model.LastName}, {model.FirstName}");
                Console.WriteLine(model.Language);
                Console.WriteLine(model.Bio);
                Console.WriteLine(model.Version);
                Console.WriteLine("");
            }
        }
    }
}
