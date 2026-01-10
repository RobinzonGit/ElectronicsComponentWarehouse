//Сервис компонентов
using AutoMapper;
using ElectronicsComponentWarehouse.Application.DTOs.Components;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using ElectronicsComponentWarehouse.Domain.Common;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Application.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с компонентами
    /// </summary>
    public class ComponentService : IComponentService
    {
        private readonly IComponentRepository _componentRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ComponentService> _logger;

        public ComponentService(
            IComponentRepository componentRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<ComponentService> logger)
        {
            _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ComponentDto>> GetAllComponentsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all components");
            
            var components = await _componentRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ComponentDto>>(components);
        }

        public async Task<ComponentDto?> GetComponentByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting component by ID: {ComponentId}", id);
            
            var component = await _componentRepository.GetByIdAsync(id, cancellationToken);
            if (component == null)
            {
                _logger.LogWarning("Component with ID {ComponentId} not found", id);
                return null;
            }
            
            return _mapper.Map<ComponentDto>(component);
        }

        public async Task<IEnumerable<ComponentDto>> GetComponentsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting components by category ID: {CategoryId}", categoryId);
            
            // Проверяем существование категории
            var categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == categoryId, cancellationToken);
            if (!categoryExists)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", categoryId);
                throw new EntityNotFoundException($"Category with ID {categoryId} not found");
            }
            
            var components = await _componentRepository.GetByCategoryIdAsync(categoryId, cancellationToken);
            return _mapper.Map<IEnumerable<ComponentDto>>(components);
        }

        public async Task<ComponentDto> CreateComponentAsync(CreateComponentDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new component: {ComponentName}", createDto.Name);
            
            // Проверяем существование категории
            var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId, cancellationToken);
            if (category == null)
            {
                _logger.LogError("Category with ID {CategoryId} not found", createDto.CategoryId);
                throw new EntityNotFoundException($"Category with ID {createDto.CategoryId} not found");
            }
            
            // Проверяем уникальность номера ячейки хранения
            var cellNumberExists = await _componentRepository.ExistsAsync(
                c => c.StorageCellNumber == createDto.StorageCellNumber, 
                cancellationToken);
            
            if (cellNumberExists)
            {
                _logger.LogError("Storage cell number {CellNumber} already exists", createDto.StorageCellNumber);
                throw new BusinessRuleException($"Storage cell number '{createDto.StorageCellNumber}' already exists");
            }
            
            // Маппим DTO в сущность
            var component = _mapper.Map<Domain.Entities.Component>(createDto);
            
            // Устанавливаем временные метки
            component.CreatedAt = DateTime.UtcNow;
            component.LastUpdated = DateTime.UtcNow;
            
            // Сохраняем компонент
            var createdComponent = await _componentRepository.AddAsync(component, cancellationToken);
            
            _logger.LogInformation("Component created successfully with ID: {ComponentId}", createdComponent.Id);
            
            return _mapper.Map<ComponentDto>(createdComponent);
        }

        public async Task<ComponentDto> UpdateComponentAsync(int id, UpdateComponentDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating component with ID: {ComponentId}", id);
            
            // Получаем существующий компонент
            var existingComponent = await _componentRepository.GetByIdAsync(id, cancellationToken);
            if (existingComponent == null)
            {
                _logger.LogError("Component with ID {ComponentId} not found", id);
                throw new EntityNotFoundException($"Component with ID {id} not found");
            }
            
            // Проверяем существование категории
            var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId, cancellationToken);
            if (category == null)
            {
                _logger.LogError("Category with ID {CategoryId} not found", updateDto.CategoryId);
                throw new EntityNotFoundException($"Category with ID {updateDto.CategoryId} not found");
            }
            
            // Проверяем уникальность номера ячейки хранения (если изменился)
            if (existingComponent.StorageCellNumber != updateDto.StorageCellNumber)
            {
                var cellNumberExists = await _componentRepository.ExistsAsync(
                    c => c.StorageCellNumber == updateDto.StorageCellNumber && c.Id != id, 
                    cancellationToken);
                
                if (cellNumberExists)
                {
                    _logger.LogError("Storage cell number {CellNumber} already exists", updateDto.StorageCellNumber);
                    throw new BusinessRuleException($"Storage cell number '{updateDto.StorageCellNumber}' already exists");
                }
            }
            
            // Маппим обновления в существующую сущность
            _mapper.Map(updateDto, existingComponent);
            
            // Обновляем временную метку (автоматически в репозитории)
            
            // Сохраняем изменения
            await _componentRepository.UpdateAsync(existingComponent, cancellationToken);
            
            _logger.LogInformation("Component with ID {ComponentId} updated successfully", id);
            
            return _mapper.Map<ComponentDto>(existingComponent);
        }

        public async Task<ComponentDto> UpdateComponentQuantityAsync(int id, UpdateComponentQuantityDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating quantity for component with ID: {ComponentId}", id);
            
            // Получаем существующий компонент
            var existingComponent = await _componentRepository.GetByIdAsync(id, cancellationToken);
            if (existingComponent == null)
            {
                _logger.LogError("Component with ID {ComponentId} not found", id);
                throw new EntityNotFoundException($"Component with ID {id} not found");
            }
            
            // Обновляем только разрешенные поля
            existingComponent.StockQuantity = updateDto.StockQuantity;
            
            if (!string.IsNullOrWhiteSpace(updateDto.DatasheetLink))
            {
                existingComponent.DatasheetLink = updateDto.DatasheetLink;
            }
            
            // Обновляем временную метку (автоматически в репозитории)
            
            // Сохраняем изменения
            await _componentRepository.UpdateAsync(existingComponent, cancellationToken);
            
            _logger.LogInformation("Quantity for component with ID {ComponentId} updated to {Quantity}", 
                id, updateDto.StockQuantity);
            
            return _mapper.Map<ComponentDto>(existingComponent);
        }

        public async Task<bool> DeleteComponentAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting component with ID: {ComponentId}", id);
            
            var result = await _componentRepository.DeleteByIdAsync(id, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Component with ID {ComponentId} deleted successfully", id);
            }
            else
            {
                _logger.LogWarning("Component with ID {ComponentId} not found for deletion", id);
            }
            
            return result;
        }

        public async Task<IEnumerable<ComponentDto>> SearchComponentsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllComponentsAsync(cancellationToken);
            }
            
            _logger.LogInformation("Searching components with term: {SearchTerm}", searchTerm);
            
            var components = await _componentRepository.SearchAsync(searchTerm, cancellationToken);
            return _mapper.Map<IEnumerable<ComponentDto>>(components);
        }

        public async Task<IEnumerable<ComponentDto>> GetLowStockComponentsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting low stock components");
            
            var components = await _componentRepository.GetLowStockComponentsAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ComponentDto>>(components);
        }

        public async Task IncreaseComponentStockAsync(int componentId, int amount, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Increasing stock for component ID {ComponentId} by {Amount}", 
                componentId, amount);
            
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            }
            
            await _componentRepository.IncreaseStockQuantityAsync(componentId, amount, cancellationToken);
            
            _logger.LogInformation("Stock for component ID {ComponentId} increased by {Amount}", 
                componentId, amount);
        }

        public async Task DecreaseComponentStockAsync(int componentId, int amount, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Decreasing stock for component ID {ComponentId} by {Amount}", 
                componentId, amount);
            
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            }
            
            await _componentRepository.DecreaseStockQuantityAsync(componentId, amount, cancellationToken);
            
            _logger.LogInformation("Stock for component ID {ComponentId} decreased by {Amount}", 
                componentId, amount);
        }

        public async Task<ComponentStatisticsDto> GetComponentsStatisticsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting components statistics");
            
            var statistics = await _componentRepository.GetStatisticsAsync(cancellationToken);
            
            // Получаем компоненты по категориям
            var allComponents = await _componentRepository.GetAllAsync(cancellationToken);
            var componentsByCategory = allComponents
                .GroupBy(c => c.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Count());
            
            return new ComponentStatisticsDto
            {
                TotalComponents = statistics.TotalComponents,
                TotalQuantity = statistics.TotalQuantity,
                LowStockCount = statistics.LowStockCount,
                TotalValue = statistics.TotalValue,
                ComponentsByCategory = componentsByCategory
            };
        }
    }
}
