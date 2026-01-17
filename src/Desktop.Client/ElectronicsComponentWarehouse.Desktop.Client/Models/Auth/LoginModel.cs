using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Desktop.Client.Models.Auth
{
    /// <summary>
    /// Модель для входа в систему
    /// </summary>
    public partial class LoginModel : ObservableValidator
    {
        [ObservableProperty]
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [MinLength(3, ErrorMessage = "Имя пользователя должно содержать минимум 3 символа")]
        [MaxLength(50, ErrorMessage = "Имя пользователя не должно превышать 50 символов")]
        [NotifyDataErrorInfo]
        private string _username = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [NotifyDataErrorInfo]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private string? _errorMessage;

        /// <summary>
        /// Проверка валидности модели
        /// </summary>
        public bool IsValid()
        {
            ValidateAllProperties();
            return !HasErrors && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Очистить поля
        /// </summary>
        public void Clear()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = null;
            ClearErrors();
        }
    }
}