//Создаем репозиторий для Component
using ElectronicsComponentWarehouse.Domain.Entities;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с компонентами
    /// </summary>
    public class ComponentRepository : BaseRepository<Component>, IComponentRepository
    {
        public ComponentRepository(
            ApplicationDbContext context,
            ILogger<ComponentRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<Component>> GetByCategoryIdAsync(
            int categoryId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting components by category ID {CategoryId}", categoryId);
            return await _dbSet
                .Where(c => c.CategoryId == categoryId)
                .Include(c => c.Category)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Component>> GetLowStockComponentsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting low stock components");
            return await _dbSet
                .Where(c => c.StockQuantity <= c.MinimumStockLevel)
                .Include(c => c.Category)
                .OrderBy(c => c.StockQuantity)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Component>> GetByManufacturerAsync(
            string manufacturer, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting components by manufacturer: {Manufacturer}", manufacturer);
            return await _dbSet
                .Where(c => c.Manufacturer != null && c.Manufacturer.ToLower().Contains(manufacturer.ToLower()))
                .Include(c => c.Category)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Component>> SearchAsync(
            string searchTerm, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(cancellationToken);

            _logger.LogDebug("Searching components with term: {SearchTerm}", searchTerm);
            
            var lowerSearchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Where(c => 
                    c.Name.ToLower().Contains(lowerSearchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(lowerSearchTerm)) ||
                    (c.Manufacturer != null && c.Manufacturer.ToLower().Contains(lowerSearchTerm)) ||
                    (c.ModelNumber != null && c.ModelNumber.ToLower().Contains(lowerSearchTerm)))
                .Include(c => c.Category)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateStockQuantityAsync(
            int componentId, 
            int newQuantity, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Updating stock quantity for component ID {ComponentId} to {NewQuantity}", 
                componentId, newQuantity);

            if (newQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(newQuantity));

            var component = await _dbSet.FindAsync(new object[] { componentId }, cancellationToken);
            if (component == null)
                throw new KeyNotFoundException($"Component with ID {componentId} not found");

            component.StockQuantity = newQuantity;
            component.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated stock quantity for component ID {ComponentId} to {NewQuantity}", 
                componentId, newQuantity);
        }

        public async Task IncreaseStockQuantityAsync(
            int componentId, 
            int amount, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Increasing stock quantity for component ID {ComponentId} by {Amount}", 
                componentId, amount);

            if (amount < 0)
                throw new ArgumentException("Amount to increase cannot be negative", nameof(amount));

            var component = await _dbSet.FindAsync(new object[] { componentId }, cancellationToken);
            if (component == null)
                throw new KeyNotFoundException($"Component with ID {componentId} not found");

            component.StockQuantity += amount;
            component.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Increased stock quantity for component ID {ComponentId} by {Amount} to {NewQuantity}", 
                componentId, amount, component.StockQuantity);
        }

        public async Task DecreaseStockQuantityAsync(
            int componentId, 
            int amount, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Decreasing stock quantity for component ID {ComponentId} by {Amount}", 
                componentId, amount);

            if (amount < 0)
                throw new ArgumentException("Amount to decrease cannot be negative", nameof(amount));

            var component = await _dbSet.FindAsync(new object[] { componentId }, cancellationToken);
            if (component == null)
                throw new KeyNotFoundException($"Component with ID {componentId} not found");

            if (component.StockQuantity < amount)
            {
                _logger.LogWarning("Insufficient stock for component ID {ComponentId}. Requested: {Amount}, Available: {Stock}", 
                    componentId, amount, component.StockQuantity);
                throw new InvalidOperationException($"Insufficient stock. Available: {component.StockQuantity}, Requested: {amount}");
            }

            component.StockQuantity -= amount;
            component.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Decreased stock quantity for component ID {ComponentId} by {Amount} to {NewQuantity}", 
                componentId, amount, component.StockQuantity);
        }

        public async Task<bool> HasSufficientStockAsync(
            int componentId, 
            int requiredAmount, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if component ID {ComponentId} has sufficient stock for {RequiredAmount}", 
                componentId, requiredAmount);

            var component = await _dbSet.FindAsync(new object[] { componentId }, cancellationToken);
            if (component == null)
                return false;

            return component.StockQuantity >= requiredAmount;
        }

        public async Task<ComponentStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting component statistics");
            
            var statistics = new ComponentStatistics();
            
            statistics.TotalComponents = await _dbSet.CountAsync(cancellationToken);
            statistics.TotalQuantity = await _dbSet.SumAsync(c => c.StockQuantity, cancellationToken);
            statistics.LowStockCount = await _dbSet.CountAsync(
                c => c.StockQuantity <= c.MinimumStockLevel, cancellationToken);
            
            // Рассчитываем общую стоимость только для компонентов с указанной ценой
            statistics.TotalValue = await _dbSet
                .Where(c => c.UnitPrice.HasValue)
                .SumAsync(c => c.UnitPrice!.Value * c.StockQuantity, cancellationToken);

            _logger.LogInformation("Component statistics calculated: {Statistics}", statistics);
            
            return statistics;
        }

        // Переопределяем методы для включения связанных данных
        public override async Task<IEnumerable<Component>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all components with category information");
            return await _dbSet
                .Include(c => c.Category)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public override async Task<Component?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting component by ID {Id} with category information", id);
            return await _dbSet
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public override async Task<IEnumerable<Component>> FindAsync(
            Expression<Func<Component, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Finding components with predicate and category information");
            return await _dbSet
                .Include(c => c.Category)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
