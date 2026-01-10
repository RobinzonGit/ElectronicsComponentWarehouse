//Интерфейс сервиса пользователей
using ElectronicsComponentWarehouse.Application.DTOs.Users;

namespace ElectronicsComponentWarehouse.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с пользователями
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить пользователя по имени пользователя
        /// </summary>
        Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        Task<UserDto> CreateUserAsync(CreateUserDto createDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Обновить пользователя
        /// </summary>
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Удалить пользователя
        /// </summary>
        Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Обновить роль пользователя
        /// </summary>
        Task<UserDto> UpdateUserRoleAsync(int id, string role, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Активировать/деактивировать пользователя
        /// </summary>
        Task<UserDto> SetUserActiveStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO для обновления пользователя
    /// </summary>
    public class UpdateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
