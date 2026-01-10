//Интерфейс сервиса категорий
using ElectronicsComponentWarehouse.Application.DTOs.Categories;

namespace ElectronicsComponentWarehouse.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с категориями компонентов
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Получить все категории
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить категорию по ID
        /// </summary>
        Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить корневые категории
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить дочерние категории
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить полную иерархию категорий
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Создать новую категорию
        /// </summary>
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Обновить существующую категорию
        /// </summary>
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Удалить категорию
        /// </summary>
        Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить путь категории (все родительские категории)
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Проверить, можно ли удалить категорию
        /// </summary>
        Task<CategoryDeletionCheckDto> CheckCategoryDeletionAsync(int categoryId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Результат проверки возможности удаления категории
    /// </summary>
    public class CategoryDeletionCheckDto
    {
        public bool CanBeDeleted { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ComponentCount { get; set; }
        public int ChildCategoryCount { get; set; }
    }
}
