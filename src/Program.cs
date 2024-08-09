using Boostlingo.Models;
using Boostlingo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


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
                .AddTransient<IDatabaseService, DatabaseService>()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                });
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
