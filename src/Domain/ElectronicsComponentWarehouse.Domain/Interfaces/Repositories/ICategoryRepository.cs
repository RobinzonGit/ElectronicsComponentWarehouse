//Создаем интерфейс репозитория для Category
using ElectronicsComponentWarehouse.Domain.Entities;

namespace ElectronicsComponentWarehouse.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий для работы с категориями компонентов
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Получить корневые категории (без родителя)
        /// </summary>
        Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все дочерние категории для указанной родительской категории
        /// </summary>
        Task<IEnumerable<Category>> GetChildCategoriesAsync(
            int parentCategoryId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить полную иерархию категорий
        /// </summary>
        Task<IEnumerable<Category>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить, имеет ли категория дочерние категории
        /// </summary>
        Task<bool> HasChildCategoriesAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить, имеет ли категория привязанные компоненты
        /// </summary>
        Task<bool> HasComponentsAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить категорию по имени
        /// </summary>
        Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить полный путь категории (все родительские категории)
        /// </summary>
        Task<IEnumerable<Category>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default);
    }
}
