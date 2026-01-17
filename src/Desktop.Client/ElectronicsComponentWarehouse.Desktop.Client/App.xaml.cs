using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    public partial class App : Application
    {
        private IHost? _host;
        public static IServiceProvider? ServiceProvider { get; private set; }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                // Создание и запуск хоста
                _host = DependencyInjection.CreateHostBuilder(e.Args).Build();
                await _host.StartAsync();

                ServiceProvider = _host.Services;

                // Показываем окно логина
                var loginWindow = ServiceProvider.GetRequiredService<Views.LoginWindow>();
                if (loginWindow.ShowDialog() == true)
                {
                    // Если логин успешен, показываем главное окно
                    var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                }
                else
                {
                    // Если отменен логин, закрываем приложение
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при запуске приложения:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
            }
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            if (_host != null)
            {
                using (_host)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}