//Создаем интерфейс репозитория для Component
using ElectronicsComponentWarehouse.Domain.Entities;

namespace ElectronicsComponentWarehouse.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий для работы с электронными компонентами
    /// </summary>
    public interface IComponentRepository : IRepository<Component>
    {
        /// <summary>
        /// Получить компоненты по категории
        /// </summary>
        Task<IEnumerable<Component>> GetByCategoryIdAsync(
            int categoryId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить компоненты с количеством ниже минимального уровня
        /// </summary>
        Task<IEnumerable<Component>> GetLowStockComponentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить компоненты по производителю
        /// </summary>
        Task<IEnumerable<Component>> GetByManufacturerAsync(
            string manufacturer, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Поиск компонентов по названию или описанию
        /// </summary>
        Task<IEnumerable<Component>> SearchAsync(
            string searchTerm, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить количество компонента на складе
        /// </summary>
        Task UpdateStockQuantityAsync(
            int componentId, 
            int newQuantity, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Увеличить количество компонента на складе
        /// </summary>
        Task IncreaseStockQuantityAsync(
            int componentId, 
            int amount, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Уменьшить количество компонента на складе
        /// </summary>
        Task DecreaseStockQuantityAsync(
            int componentId, 
            int amount, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить, достаточно ли компонентов на складе
        /// </summary>
        Task<bool> HasSufficientStockAsync(
            int componentId, 
            int requiredAmount, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить статистику по компонентам
        /// </summary>
        Task<ComponentStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Статистика по компонентам
    /// </summary>
    public class ComponentStatistics
    {
        public int TotalComponents { get; set; }
        public int TotalQuantity { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalValue { get; set; }
    }
}
