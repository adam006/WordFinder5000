using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using WordFinder5000.Core;

namespace WordFinder5000.ConsoleApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHost();

            var service = host.Services.GetRequiredService<IBookService>();

            var result = await service.GetTopWordsAsync();

            Console.WriteLine("Top words:");

            foreach (var s in result)
            {
                Console.WriteLine(s);
            }
        }

        private static IHost CreateHost()
        {
            var host = CreateHostBuilder().Build();
            ConfigureLogger();
            return host;
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    RegisterServices(services, hostContext.Configuration);
                })
                .UseSerilog();
        }

        private static void ConfigureLogger()
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            Log.Logger = loggerConfig.CreateLogger();
        }

        private static void RegisterServices(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IBookRepo, BookRepo>();
            services.AddSingleton<IBookService, BookService>();
            services.AddSingleton<IFilerParser, FileParser>();
            services.AddHttpClient<IBookRepo, BookRepo>();

            var appSettings = new AppSettings();

            config.Bind(appSettings);

            services.AddSingleton(appSettings);
        }
    }
}