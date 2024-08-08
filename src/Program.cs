using Boostlingo.Models;
using Boostlingo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Boostlingo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();
            var dataProcessor = serviceProvider.GetRequiredService<DataProcessor>();

            await dataProcessor.ProcessData();

            Console.WriteLine("Operation has completed successfully and will now exit.");
            Environment.Exit(0);

        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = LoadConfiguration();
            services
                .AddHttpClient()
                .AddSingleton(configuration)
                .AddTransient<DataProcessor>()
                .AddTransient<IJsonDataService, JsonDataService>()
                .AddTransient<IDatabaseService, DatabaseService>();
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }


    }
}
