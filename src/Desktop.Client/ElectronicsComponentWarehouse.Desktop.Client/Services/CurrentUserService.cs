using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Auth;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using System;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services
{
    /// <summary>
    /// Сервис для управления текущим пользователем и его состоянием
    /// </summary>
    public class CurrentUserService : ObservableObject
    {
        private readonly IAuthService _authService;
        private UserInfoModel? _currentUser;

        public UserInfoModel? CurrentUser
        {
            get => _currentUser;
            private set => SetProperty(ref _currentUser, value);
        }

        public bool IsAuthenticated => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
        public bool IsRegularUser => CurrentUser?.IsRegularUser ?? false;

        public event EventHandler? UserChanged;

        public CurrentUserService(IAuthService authService)
        {
            _authService = authService;
            _authService.AuthenticationChanged += OnAuthenticationChanged;

            // Инициализация текущего пользователя
            CurrentUser = _authService.CurrentUser;
        }

        private void OnAuthenticationChanged(object? sender, bool isAuthenticated)
        {
            CurrentUser = _authService.CurrentUser;
            UserChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Обновить информацию о текущем пользователе
        /// </summary>
        public void RefreshUserInfo()
        {
            CurrentUser = _authService.CurrentUser;
            UserChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Выйти из системы
        /// </summary>
        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            CurrentUser = null;
            UserChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Проверить, есть ли у пользователя указанная роль
        /// </summary>
        public bool HasRole(string role)
        {
            return CurrentUser?.Role.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Проверить, есть ли у пользователя любая из указанных ролей
        /// </summary>
        public bool HasAnyRole(params string[] roles)
        {
            if (CurrentUser == null) return false;

            foreach (var role in roles)
            {
                if (CurrentUser.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}