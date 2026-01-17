using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ElectronicsComponentWarehouse.Desktop.Client.Models.Users
{
    /// <summary>
    /// Модель пользователя для клиентского приложения
    /// </summary>
    public partial class UserModel : ObservableObject
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

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private DateTime? _lastLoginAt;

        [ObservableProperty]
        private bool _isActive;

        // Вычисляемые свойства
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public bool IsRegularUser => Role.Equals("User", StringComparison.OrdinalIgnoreCase);

        public string Status => IsActive ? "Активен" : "Неактивен";

        public string LastLoginFormatted => LastLoginAt?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда";
    }
}