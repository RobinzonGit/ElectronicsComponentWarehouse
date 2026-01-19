using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public LoginWindow(IAuthService authService, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            Loaded += LoginWindow_Loaded;
        }

        private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Пытаемся восстановить сессию
                var hasSession = await _authService.LoadSavedSessionAsync();

                if (hasSession && _authService.IsAuthenticated)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading saved session");
            }
        }

        private void UpdateLoginButtonState()
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            LoginButton.IsEnabled = !string.IsNullOrEmpty(username) &&
                                   !string.IsNullOrEmpty(password) &&
                                   username.Length >= 3 &&
                                   password.Length >= 6;
        }

        private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateLoginButtonState();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateLoginButtonState();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password");
                return;
            }

            try
            {
                LoginButton.IsEnabled = false;
                ShowError(null);

                var authResponse = await _authService.LoginAsync(username, password);

                if (authResponse != null)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Invalid username or password");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login error for user {Username}", username);
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string? message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ErrorBorder.Visibility = Visibility.Collapsed;
                ErrorMessageText.Text = string.Empty;
            }
            else
            {
                ErrorBorder.Visibility = Visibility.Visible;
                ErrorMessageText.Text = message;
            }
        }
    }
}