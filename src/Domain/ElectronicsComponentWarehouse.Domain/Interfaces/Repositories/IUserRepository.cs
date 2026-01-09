//Создаем интерфейс репозитория для User
using ElectronicsComponentWarehouse.Domain.Entities;

namespace ElectronicsComponentWarehouse.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Найти пользователя по имени пользователя
        /// </summary>
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Найти пользователя по email
        /// </summary>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование пользователя с указанным именем
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование пользователя с указанным email
        /// </summary>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить время последнего входа пользователя
        /// </summary>
        Task UpdateLastLoginAsync(int userId, DateTime loginTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить всех пользователей с указанной ролью
        /// </summary>
        Task<IEnumerable<User>> GetByRoleAsync(Domain.Enums.UserRole role, CancellationToken cancellationToken = default);
    }
}
