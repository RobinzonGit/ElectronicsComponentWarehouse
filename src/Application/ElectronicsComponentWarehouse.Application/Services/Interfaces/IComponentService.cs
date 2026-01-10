//Интерфейс сервиса компонентов
using ElectronicsComponentWarehouse.Application.DTOs.Components;

namespace ElectronicsComponentWarehouse.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с электронными компонентами
    /// </summary>
    public interface IComponentService
    {
        /// <summary>
        /// Получить все компоненты
        /// </summary>
        Task<IEnumerable<ComponentDto>> GetAllComponentsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить компонент по ID
        /// </summary>
        Task<ComponentDto?> GetComponentByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить компоненты по категории
        /// </summary>
        Task<IEnumerable<ComponentDto>> GetComponentsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Создать новый компонент
        /// </summary>
        Task<ComponentDto> CreateComponentAsync(CreateComponentDto createDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Обновить существующий компонент
        /// </summary>
        Task<ComponentDto> UpdateComponentAsync(int id, UpdateComponentDto updateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Обновить только количество компонента и ссылку на документацию
        /// </summary>
        Task<ComponentDto> UpdateComponentQuantityAsync(int id, UpdateComponentQuantityDto updateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Удалить компонент
        /// </summary>
        Task<bool> DeleteComponentAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Поиск компонентов
        /// </summary>
        Task<IEnumerable<ComponentDto>> SearchComponentsAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить компоненты с низким запасом
        /// </summary>
        Task<IEnumerable<ComponentDto>> GetLowStockComponentsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Увеличить количество компонента
        /// </summary>
        Task IncreaseComponentStockAsync(int componentId, int amount, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Уменьшить количество компонента
        /// </summary>
        Task DecreaseComponentStockAsync(int componentId, int amount, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить статистику по компонентам
        /// </summary>
        Task<ComponentStatisticsDto> GetComponentsStatisticsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO для статистики компонентов
    /// </summary>
    public class ComponentStatisticsDto
    {
        public int TotalComponents { get; set; }
        public int TotalQuantity { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalValue { get; set; }
        public Dictionary<string, int> ComponentsByCategory { get; set; } = new();
    }
}
