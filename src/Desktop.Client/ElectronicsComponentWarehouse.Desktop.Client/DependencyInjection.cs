using ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using ElectronicsComponentWarehouse.Desktop.Client.ViewModels;
using ElectronicsComponentWarehouse.Desktop.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
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

                    // Настройка Serilog
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File(
                            path: "logs/client-.log",
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7,
                            shared: true)
                        .CreateLogger();

                    // Регистрация сервисов
                    ConfigureServices(services, configuration);

                    // Регистрация ViewModels и Views
                    ConfigureViewModels(services);
                    ConfigureViews(services);
                });
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // HTTP клиент
            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(
                    int.Parse(configuration["ApiSettings:TimeoutSeconds"] ?? "30"));
            });

            // Сервисы приложения
            services.AddSingleton<ILocalStorageService, JsonFileStorageService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IComponentService, ComponentService>();
            services.AddSingleton<ICategoryService, CategoryService>();

            // Конфигурация
            services.AddSingleton(configuration);

            // Логгер
            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });
        }

        private static void ConfigureViewModels(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
        }

        private static void ConfigureViews(IServiceCollection services)
        {
            // Главное окно
            services.AddSingleton<MainWindow>();
            services.AddTransient<Views.LoginWindow>();
        }
    }
}