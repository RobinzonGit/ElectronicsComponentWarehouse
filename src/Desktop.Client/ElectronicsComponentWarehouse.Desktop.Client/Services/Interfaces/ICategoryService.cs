using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с категориями
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Получить все категории
        /// </summary>
        Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить категорию по ID
        /// </summary>
        Task<CategoryModel?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить корневые категории
        /// </summary>
        Task<IEnumerable<CategoryModel>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить дочерние категории
        /// </summary>
        Task<IEnumerable<CategoryModel>> GetChildCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить полную иерархию категорий
        /// </summary>
        Task<IEnumerable<CategoryModel>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Создать категорию
        /// </summary>
        Task<CategoryModel?> CreateCategoryAsync(CategoryModel category, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить категорию
        /// </summary>
        Task<CategoryModel?> UpdateCategoryAsync(CategoryModel category, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить категорию
        /// </summary>
        Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить путь категории
        /// </summary>
        Task<IEnumerable<CategoryModel>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить возможность удаления категории
        /// </summary>
        Task<CategoryDeletionCheck> CheckCategoryDeletionAsync(int categoryId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Результат проверки удаления категории
    /// </summary>
    public class CategoryDeletionCheck
    {
        public bool CanBeDeleted { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ComponentCount { get; set; }
        public int ChildCategoryCount { get; set; }
    }
}