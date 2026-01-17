using System.Text.Json;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с компонентами
    /// </summary>
    public class ComponentService : IComponentService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<ComponentService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ComponentService(IApiClient apiClient, ILogger<ComponentService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<IEnumerable<ComponentModel>> GetAllComponentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение всех компонентов");

                var components = await _apiClient.GetAsync<IEnumerable<ComponentModel>>(
                    "api/Components",
                    cancellationToken);

                return components ?? Enumerable.Empty<ComponentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех компонентов");
                throw;
            }
        }

        public async Task<ComponentModel?> GetComponentByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение компонента по ID: {ComponentId}", id);

                return await _apiClient.GetAsync<ComponentModel>(
                    $"api/Components/{id}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении компонента по ID: {ComponentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ComponentModel>> GetComponentsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение компонентов по категории ID: {CategoryId}", categoryId);

                var components = await _apiClient.GetAsync<IEnumerable<ComponentModel>>(
                    $"api/Components/category/{categoryId}",
                    cancellationToken);

                return components ?? Enumerable.Empty<ComponentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении компонентов по категории ID: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<ComponentModel?> CreateComponentAsync(ComponentModel component, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Создание компонента: {ComponentName}", component.Name);

                var createDto = new
                {
                    component.Name,
                    component.Description,
                    component.StockQuantity,
                    component.StorageCellNumber,
                    component.Manufacturer,
                    component.ModelNumber,
                    component.DatasheetLink,
                    component.MinimumStockLevel,
                    component.UnitPrice,
                    component.CategoryId
                };

                return await _apiClient.PostAsync<ComponentModel>(
                    "api/Components",
                    createDto,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании компонента: {ComponentName}", component.Name);
                throw;
            }
        }

        public async Task<ComponentModel?> UpdateComponentAsync(ComponentModel component, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Обновление компонента ID: {ComponentId}", component.Id);

                var updateDto = new
                {
                    component.Name,
                    component.Description,
                    component.StockQuantity,
                    component.StorageCellNumber,
                    component.Manufacturer,
                    component.ModelNumber,
                    component.DatasheetLink,
                    component.MinimumStockLevel,
                    component.UnitPrice,
                    component.CategoryId
                };

                return await _apiClient.PutAsync<ComponentModel>(
                    $"api/Components/{component.Id}",
                    updateDto,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении компонента ID: {ComponentId}", component.Id);
                throw;
            }
        }

        public async Task<ComponentModel?> UpdateComponentQuantityAsync(int id, int quantity, string? datasheetLink, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Обновление количества компонента ID: {ComponentId}", id);

                var updateDto = new
                {
                    stockQuantity = quantity,
                    datasheetLink
                };

                return await _apiClient.PatchAsync<ComponentModel>(
                    $"api/Components/{id}/quantity",
                    updateDto,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении количества компонента ID: {ComponentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteComponentAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Удаление компонента ID: {ComponentId}", id);

                return await _apiClient.DeleteAsync(
                    $"api/Components/{id}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении компонента ID: {ComponentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ComponentModel>> SearchComponentsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Поиск компонентов: {SearchTerm}", searchTerm);

                var encodedTerm = Uri.EscapeDataString(searchTerm);
                var components = await _apiClient.GetAsync<IEnumerable<ComponentModel>>(
                    $"api/Components/search?searchTerm={encodedTerm}",
                    cancellationToken);

                return components ?? Enumerable.Empty<ComponentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске компонентов: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<ComponentModel>> GetLowStockComponentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение компонентов с низким запасом");

                var components = await _apiClient.GetAsync<IEnumerable<ComponentModel>>(
                    "api/Components/low-stock",
                    cancellationToken);

                return components ?? Enumerable.Empty<ComponentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении компонентов с низким запасом");
                throw;
            }
        }

        public async Task<ComponentStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Получение статистики компонентов");

                var statistics = await _apiClient.GetAsync<ComponentStatistics>(
                    "api/Components/statistics",
                    cancellationToken);

                return statistics ?? new ComponentStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики компонентов");
                throw;
            }
        }
    }
}