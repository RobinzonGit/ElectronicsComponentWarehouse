using ElectronicsComponentWarehouse.Desktop.Client.Models.Auth;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces
{
    /// <summary>
    /// Сервис аутентификации
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Текущий пользователь
        /// </summary>
        UserInfoModel? CurrentUser { get; }

        /// <summary>
        /// Токен авторизации
        /// </summary>
        string? Token { get; }

        /// <summary>
        /// Авторизован ли пользователь
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Является ли пользователь администратором
        /// </summary>
        bool IsAdmin { get; }

        /// <summary>
        /// Событие изменения статуса авторизации
        /// </summary>
        event EventHandler<bool>? AuthenticationChanged;

        /// <summary>
        /// Войти в систему
        /// </summary>
        Task<AuthResponseModel?> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выйти из системы
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Проверить валидность токена
        /// </summary>
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить токен
        /// </summary>
        Task<string?> RefreshTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Загрузить сохраненную сессию
        /// </summary>
        Task<bool> LoadSavedSessionAsync();

        /// <summary>
        /// Сохранить текущую сессию
        /// </summary>
        Task SaveSessionAsync();

        /// <summary>
        /// Очистить сохраненную сессию
        /// </summary>
        Task ClearSavedSessionAsync();
    }
}