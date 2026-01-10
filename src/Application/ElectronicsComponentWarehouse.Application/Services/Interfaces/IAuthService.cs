//Интерфейс сервиса аутентификации
using ElectronicsComponentWarehouse.Application.DTOs.Auth;
using ElectronicsComponentWarehouse.Application.DTOs.Users;

namespace ElectronicsComponentWarehouse.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для аутентификации и авторизации
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(CreateUserDto registerDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Изменение пароля пользователя
        /// </summary>
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Валидация JWT токена
        /// </summary>
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получение информации о пользователе из токена
        /// </summary>
        Task<UserInfoDto?> GetUserFromTokenAsync(string token, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Обновление токена
        /// </summary>
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
