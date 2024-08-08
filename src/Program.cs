using boostlingo.models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace boostlingo
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
                })
                .Build();

            var dataService = host.Services.GetRequiredService<JsonDataService>();

            // TODO store this in config
            var url = "https://microsoftedge.github.io/Demos/json-dummy-data/64KB.json";

            string response;
            try
            {
                response = await dataService.GetJsonDataAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching JSON data:", ex);
            }

            if (string.IsNullOrEmpty(response))
            {
                throw new Exception("Response was null or empty.");
            }

            // Deserialize json into List of DummyModels
            var models = JsonConvert.DeserializeObject<List<DummyModel>>(response);
            
            if (models != null && models.Count > 0)
            {
                foreach (var model in models)
                {
                    Console.WriteLine(model.Name);
                }
            }
            else
            {
                Console.WriteLine("No Models to write");
            }

        }
    }
}
