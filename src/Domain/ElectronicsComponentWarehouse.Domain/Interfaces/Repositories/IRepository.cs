//Создаем базовый интерфейс репозитория
using System.Linq.Expressions;

namespace ElectronicsComponentWarehouse.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Базовый интерфейс репозитория для работы с сущностями
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Получить все сущности
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить сущность по ID
        /// </summary>
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Найти сущности по условию
        /// </summary>
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавить новую сущность
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить существующую сущность
        /// </summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить сущность
        /// </summary>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить сущность по ID
        /// </summary>
        Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование сущности по условию
        /// </summary>
        Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить количество сущностей
        /// </summary>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить количество сущностей по условию
        /// </summary>
        Task<int> CountAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);
    }
}
