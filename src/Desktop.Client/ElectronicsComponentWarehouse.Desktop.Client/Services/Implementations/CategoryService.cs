using System.Text.Json;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с категориями
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<CategoryService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CategoryService(IApiClient apiClient, ILogger<CategoryService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение всех категорий");

                var categories = await _apiClient.GetAsync<IEnumerable<CategoryModel>>(
                    "api/Categories",
                    cancellationToken);

                return categories ?? Enumerable.Empty<CategoryModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех категорий");
                throw;
            }
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение категории по ID: {CategoryId}", id);

                return await _apiClient.GetAsync<CategoryModel>(
                    $"api/Categories/{id}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении категории по ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryModel>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение корневых категорий");

                var categories = await _apiClient.GetAsync<IEnumerable<CategoryModel>>(
                    "api/Categories/roots",
                    cancellationToken);

                return categories ?? Enumerable.Empty<CategoryModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении корневых категорий");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryModel>> GetChildCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение дочерних категорий для родителя ID: {ParentCategoryId}", parentCategoryId);

                var categories = await _apiClient.GetAsync<IEnumerable<CategoryModel>>(
                    $"api/Categories/parent/{parentCategoryId}",
                    cancellationToken);

                return categories ?? Enumerable.Empty<CategoryModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении дочерних категорий для родителя ID: {ParentCategoryId}", parentCategoryId);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение иерархии категорий");

                var categories = await _apiClient.GetAsync<IEnumerable<CategoryModel>>(
                    "api/Categories/hierarchy",
                    cancellationToken);

                return categories ?? Enumerable.Empty<CategoryModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении иерархии категорий");
                throw;
            }
        }

        public async Task<CategoryModel?> CreateCategoryAsync(CategoryModel category, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Создание категории: {CategoryName}", category.Name);

                var createDto = new
                {
                    category.Name,
                    category.Description,
                    category.ParentCategoryId
                };

                return await _apiClient.PostAsync<CategoryModel>(
                    "api/Categories",
                    createDto,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании категории: {CategoryName}", category.Name);
                throw;
            }
        }

        public async Task<CategoryModel?> UpdateCategoryAsync(CategoryModel category, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Обновление категории ID: {CategoryId}", category.Id);

                var updateDto = new
                {
                    category.Name,
                    category.Description,
                    category.ParentCategoryId
                };

                return await _apiClient.PutAsync<CategoryModel>(
                    $"api/Categories/{category.Id}",
                    updateDto,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении категории ID: {CategoryId}", category.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Удаление категории ID: {CategoryId}", id);

                return await _apiClient.DeleteAsync(
                    $"api/Categories/{id}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении категории ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение пути категории ID: {CategoryId}", categoryId);

                var path = await _apiClient.GetAsync<IEnumerable<CategoryModel>>(
                    $"api/Categories/{categoryId}/path",
                    cancellationToken);

                return path ?? Enumerable.Empty<CategoryModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пути категории ID: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<CategoryDeletionCheck> CheckCategoryDeletionAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Проверка возможности удаления категории ID: {CategoryId}", categoryId);

                var check = await _apiClient.GetAsync<CategoryDeletionCheck>(
                    $"api/Categories/{categoryId}/can-delete",
                    cancellationToken);

                return check ?? new CategoryDeletionCheck
                {
                    CanBeDeleted = false,
                    Message = "Не удалось проверить возможность удаления"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке возможности удаления категории ID: {CategoryId}", categoryId);
                throw;
            }
        }
    }
}