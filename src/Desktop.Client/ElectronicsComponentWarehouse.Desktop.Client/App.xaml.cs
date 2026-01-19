using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    public partial class App : Application
    {
        private IHost? _host;

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                // Создание и запуск хоста
                _host = DependencyInjection.CreateHostBuilder(e.Args).Build();
                await _host.StartAsync();

                // Показываем главное окно
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
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