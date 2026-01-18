using CommunityToolkit.Mvvm.Messaging;
using ElectronicsComponentWarehouse.Desktop.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

                // Подписываемся на сообщения об аутентификации
                WeakReferenceMessenger.Default.Register<AuthenticationSuccessMessage>(this, OnAuthenticationSuccess);

                // Проверяем сохраненную сессию
                var authService = ServiceProvider.GetRequiredService<Services.Interfaces.IAuthService>();
                var sessionLoaded = await authService.LoadSavedSessionAsync();

                if (sessionLoaded)
                {
                    // Если сессия загружена, показываем главное окно
                    ShowMainWindow();
                }
                else
                {
                    // Иначе показываем окно входа
                    ShowLoginWindow();
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

        private void ShowLoginWindow()
        {
            var loginWindow = ServiceProvider?.GetRequiredService<Views.LoginWindow>();
            if (loginWindow != null)
            {
                loginWindow.Show();
            }
        }

        private void ShowMainWindow()
        {
            var mainWindow = ServiceProvider?.GetRequiredService<MainWindow>();
            if (mainWindow != null)
            {
                mainWindow.Show();
            }
        }

        private void OnAuthenticationSuccess(object recipient, AuthenticationSuccessMessage message)
        {
            if (message.IsAuthenticated)
            {
                // Закрываем окно входа
                foreach (Window window in Windows)
                {
                    if (window is Views.LoginWindow)
                    {
                        window.Close();
                        break;
                    }
                }

                // Показываем главное окно
                ShowMainWindow();
            }
            else
            {
                // Если аутентификация не удалась, закрываем приложение
                Shutdown();
            }
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            // Отписываемся от сообщений
            WeakReferenceMessenger.Default.Unregister<AuthenticationSuccessMessage>(this);

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