using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            var url = "https://microsoftedge.github.io/Demos/json-dummy-data/64KB.json";
            var response = await dataService.GetJsonDataAsync(url);
            Console.WriteLine(response);

        }
    }
}
