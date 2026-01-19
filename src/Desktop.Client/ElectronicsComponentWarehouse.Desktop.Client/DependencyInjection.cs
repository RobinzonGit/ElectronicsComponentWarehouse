using ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.IO;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    /// <summary>
    /// Конфигурация Dependency Injection
    /// </summary>
    public static class DependencyInjection
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    // Регистрация сервисов
                    ConfigureServices(services, configuration);

                    // Регистрация окон и представлений
                    ConfigureViews(services);
                })
                .UseSerilog((context, services, configuration) =>
                {
                    configuration
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.File(
                            path: "logs/client-.log",
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
                });
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // HTTP клиент
            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new System.Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001");
                client.Timeout = System.TimeSpan.FromSeconds(
                    int.Parse(configuration["ApiSettings:TimeoutSeconds"] ?? "30"));
            });

            // Сервисы приложения
            services.AddSingleton<ILocalStorageService, JsonFileStorageService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IComponentService, ComponentService>();
            services.AddSingleton<ICategoryService, CategoryService>();

            // Конфигурация
            services.AddSingleton(configuration);
        }

        private static void ConfigureViews(IServiceCollection services)
        {
            // Главные окна
            services.AddSingleton<MainWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<ComponentEditWindow>();
        }
    }
}