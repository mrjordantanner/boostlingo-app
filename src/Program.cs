using Boostlingo.Models;
using Boostlingo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Reflection;

namespace Boostlingo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient();
                    services.AddTransient<JsonDataService>();
                    services.AddTransient<DatabaseService>();
                })
                .Build();

            var jsonDataService = host.Services.GetRequiredService<JsonDataService>();
            var databaseService = host.Services.GetRequiredService<DatabaseService>();

            // TODO store this in config
            var url = "https://microsoftedge.github.io/Demos/json-dummy-data/64KB.json";
            var tableName = "DummyData";

            string response;
            try
            {
                response = await jsonDataService.GetJsonDataAsync(url);
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

            // Split Name field into First and Last Names
            var processedModels = SplitNames(models);

            // FOR TESTING, clear table to avoid duplicate records
            await databaseService.ClearTableAsync(tableName);

            // Insert into database table
            await databaseService.WriteDummyDataAsync(processedModels);

            // Read from database table
            var dbResponse = await databaseService.ReadDummyDataAsync();
            if (dbResponse == null || dbResponse.Count == 0)
            {
                throw new Exception($"Error reading data from {tableName} table.");
            }

            // Sort data by First/Last name and output to console
            var sortedModels = dbResponse.OrderBy(model => model.LastName)
                         .ThenBy(model => model.FirstName)
                         .ToList();

            sortedModels.ForEach(model => Console.WriteLine($"{model.LastName}, {model.FirstName}"));

        }

        public static List<DummyModel> SplitNames(List<DummyModel> models)
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
