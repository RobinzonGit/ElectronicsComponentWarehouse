using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using System;

namespace ElectronicsComponentWarehouse.Desktop.Client.Models.Auth
{
    /// <summary>
    /// Модель ответа аутентификации
    /// </summary>
    public partial class AuthResponseModel : ObservableObject
    {
        [ObservableProperty]
        private string _token = string.Empty;

        [ObservableProperty]
        private DateTime _expiresAt;

        [ObservableProperty]
        private UserInfoModel _user = new();

        // Вычисляемые свойства
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsValid => !string.IsNullOrEmpty(Token) && !IsExpired;

        /// <summary>
        /// Время до истечения токена
        /// </summary>
        public TimeSpan TimeUntilExpiry => ExpiresAt - DateTime.UtcNow;

        /// <summary>
        /// Требуется ли обновление токена (менее 5 минут до истечения)
        /// </summary>
        public bool NeedsRefresh => TimeUntilExpiry <= TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public partial class UserInfoModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _role = string.Empty;

        // Вычисляемые свойства
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public bool IsRegularUser => Role.Equals("User", StringComparison.OrdinalIgnoreCase);

        public string DisplayName => string.IsNullOrEmpty(FullName) ? Username : $"{FullName} ({Username})";
    }
}