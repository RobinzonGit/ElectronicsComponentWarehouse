using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с компонентами
    /// </summary>
    public interface IComponentService
    {
        /// <summary>
        /// Получить все компоненты
        /// </summary>
        Task<IEnumerable<ComponentModel>> GetAllComponentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить компонент по ID
        /// </summary>
        Task<ComponentModel?> GetComponentByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить компоненты по категории
        /// </summary>
        Task<IEnumerable<ComponentModel>> GetComponentsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Создать компонент
        /// </summary>
        Task<ComponentModel?> CreateComponentAsync(ComponentModel component, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить компонент
        /// </summary>
        Task<ComponentModel?> UpdateComponentAsync(ComponentModel component, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить только количество компонента
        /// </summary>
        Task<ComponentModel?> UpdateComponentQuantityAsync(int id, int quantity, string? datasheetLink, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить компонент
        /// </summary>
        Task<bool> DeleteComponentAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Поиск компонентов
        /// </summary>
        Task<IEnumerable<ComponentModel>> SearchComponentsAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить компоненты с низким запасом
        /// </summary>
        Task<IEnumerable<ComponentModel>> GetLowStockComponentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить статистику компонентов
        /// </summary>
        Task<ComponentStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Статистика компонентов
    /// </summary>
    public class ComponentStatistics
    {
        public int TotalComponents { get; set; }
        public int TotalQuantity { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalValue { get; set; }
    }
}