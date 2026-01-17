using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;

namespace ElectronicsComponentWarehouse.Desktop.Client.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;

        public LoginWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            LoginButton.Click += LoginButton_Click;
            CancelButton.Click += CancelButton_Click;

            // Пытаемся загрузить сохраненную сессию
            LoadSavedSession();
        }

        private async void LoadSavedSession()
        {
            try
            {
                if (await _authService.LoadSavedSessionAsync())
                {
                    // Если сессия загружена, закрываем окно
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки сессии: {ex.Message}");
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите имя пользователя и пароль");
                return;
            }

            try
            {
                var result = await _authService.LoginAsync(username, password);

                if (result != null)
                {
                    // Сохраняем сессию, если отмечен чекбокс
                    if (RememberCheckBox.IsChecked == true)
                    {
                        await _authService.SaveSessionAsync();
                    }

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Неверное имя пользователя или пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка входа: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}