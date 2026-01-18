// LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Auth;
using ElectronicsComponentWarehouse.Desktop.Client.Services;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels
{
    /// <summary>
    /// ViewModel для окна входа в систему
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe = true;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _showErrorMessage;

        public LoginViewModel(IAuthService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;

            // Загружаем сохраненные учетные данные, если есть
            LoadSavedCredentials();
        }

        /// <summary>
        /// Команда входа в систему
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                ShowErrorMessage = false;

                var result = await _authService.LoginAsync(Username, Password);

                if (result != null && result.IsValid)
                {
                    // Сохраняем сессию, если выбрано "Запомнить меня"
                    if (RememberMe)
                    {
                        await _authService.SaveSessionAsync();
                    }

                    // Отправляем сообщение об успешной аутентификации
                    WeakReferenceMessenger.Default.Send(new AuthenticationSuccessMessage(true));
                }
                else
                {
                    ErrorMessage = "Неверное имя пользователя или пароль";
                    ShowErrorMessage = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка входа: {ex.Message}";
                ShowErrorMessage = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsBusy;
        }

        /// <summary>
        /// Команда отмены входа
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            // Отправляем сообщение об отмене аутентификации
            WeakReferenceMessenger.Default.Send(new AuthenticationSuccessMessage(false));
        }

        /// <summary>
        /// Загрузка сохраненных учетных данных
        /// </summary>
        private async void LoadSavedCredentials()
        {
            try
            {
                var sessionLoaded = await _authService.LoadSavedSessionAsync();
                if (sessionLoaded && _authService.CurrentUser != null)
                {
                    Username = _authService.CurrentUser.Username;
                    // Пароль не загружается из соображений безопасности
                }
            }
            catch
            {
                // Игнорируем ошибки при загрузке сохраненной сессии
            }
        }

        /// <summary>
        /// Очистка полей формы
        /// </summary>
        [RelayCommand]
        private void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            ShowErrorMessage = false;
        }
    }

    /// <summary>
    /// Сообщение об успешной аутентификации
    /// </summary>
    public class AuthenticationSuccessMessage
    {
        public bool IsAuthenticated { get; }

        public AuthenticationSuccessMessage(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
    }
}