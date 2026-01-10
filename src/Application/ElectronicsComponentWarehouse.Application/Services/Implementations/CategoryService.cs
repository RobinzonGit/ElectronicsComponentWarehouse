//Сервис категорий
using AutoMapper;
using ElectronicsComponentWarehouse.Application.DTOs.Categories;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using ElectronicsComponentWarehouse.Domain.Common;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Application.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с категориями
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IComponentRepository _componentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IComponentRepository componentRepository,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all categories");
            
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Заполняем дополнительные поля
            foreach (var categoryDto in categoryDtos)
            {
                await FillCategoryCountsAsync(categoryDto, cancellationToken);
            }
            
            return categoryDtos;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting category by ID: {CategoryId}", id);
            
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return null;
            }
            
            var categoryDto = _mapper.Map<CategoryDto>(category);
            await FillCategoryCountsAsync(categoryDto, cancellationToken);
            
            return categoryDto;
        }

        public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting root categories");
            
            var categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Заполняем дополнительные поля
            foreach (var categoryDto in categoryDtos)
            {
                await FillCategoryCountsAsync(categoryDto, cancellationToken);
            }
            
            return categoryDtos;
        }

        public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting child categories for parent ID: {ParentCategoryId}", parentCategoryId);
            
            var categories = await _categoryRepository.GetChildCategoriesAsync(parentCategoryId, cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Заполняем дополнительные поля
            foreach (var categoryDto in categoryDtos)
            {
                await FillCategoryCountsAsync(categoryDto, cancellationToken);
            }
            
            return categoryDtos;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting full category hierarchy");
            
            var categories = await _categoryRepository.GetCategoryHierarchyAsync(cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Рекурсивно заполняем дополнительные поля
            foreach (var categoryDto in categoryDtos)
            {
                await FillCategoryHierarchyCountsAsync(categoryDto, cancellationToken);
            }
            
            return categoryDtos;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new category: {CategoryName}", createDto.Name);
            
            // Проверяем уникальность имени категории
            var categoryExists = await _categoryRepository.ExistsAsync(
                c => c.Name.ToLower() == createDto.Name.ToLower(), 
                cancellationToken);
            
            if (categoryExists)
            {
                _logger.LogError("Category with name {CategoryName} already exists", createDto.Name);
                throw new BusinessRuleException($"Category with name '{createDto.Name}' already exists");
            }
            
            // Проверяем родительскую категорию (если указана)
            if (createDto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(
                    createDto.ParentCategoryId.Value, 
                    cancellationToken);
                
                if (parentCategory == null)
                {
                    _logger.LogError("Parent category with ID {ParentCategoryId} not found", 
                        createDto.ParentCategoryId.Value);
                    throw new EntityNotFoundException(
                        $"Parent category with ID {createDto.ParentCategoryId.Value} not found");
                }
            }
            
            // Маппим DTO в сущность
            var category = _mapper.Map<Domain.Entities.Category>(createDto);
            
            // Устанавливаем временные метки
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            
            // Сохраняем категорию
            var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);
            
            _logger.LogInformation("Category created successfully with ID: {CategoryId}", createdCategory.Id);
            
            var categoryDto = _mapper.Map<CategoryDto>(createdCategory);
            await FillCategoryCountsAsync(categoryDto, cancellationToken);
            
            return categoryDto;
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating category with ID: {CategoryId}", id);
            
            // Получаем существующую категорию
            var existingCategory = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (existingCategory == null)
            {
                _logger.LogError("Category with ID {CategoryId} not found", id);
                throw new EntityNotFoundException($"Category with ID {id} not found");
            }
            
            // Проверяем уникальность имени категории (если изменилось)
            if (existingCategory.Name != updateDto.Name)
            {
                var categoryExists = await _categoryRepository.ExistsAsync(
                    c => c.Name.ToLower() == updateDto.Name.ToLower() && c.Id != id, 
                    cancellationToken);
                
                if (categoryExists)
                {
                    _logger.LogError("Category with name {CategoryName} already exists", updateDto.Name);
                    throw new BusinessRuleException($"Category with name '{updateDto.Name}' already exists");
                }
            }
            
            // Проверяем родительскую категорию (если указана)
            if (updateDto.ParentCategoryId.HasValue)
            {
                // Нельзя сделать категорию родителем самой себе
                if (updateDto.ParentCategoryId.Value == id)
                {
                    _logger.LogError("Category cannot be parent of itself");
                    throw new BusinessRuleException("Category cannot be parent of itself");
                }
                
                var parentCategory = await _categoryRepository.GetByIdAsync(
                    updateDto.ParentCategoryId.Value, 
                    cancellationToken);
                
                if (parentCategory == null)
                {
                    _logger.LogError("Parent category with ID {ParentCategoryId} not found", 
                        updateDto.ParentCategoryId.Value);
                    throw new EntityNotFoundException(
                        $"Parent category with ID {updateDto.ParentCategoryId.Value} not found");
                }
                
                // Проверяем циклические зависимости (нельзя создать цикл)
                var categoryPath = await _categoryRepository.GetCategoryPathAsync(
                    updateDto.ParentCategoryId.Value, 
                    cancellationToken);
                
                if (categoryPath.Any(c => c.Id == id))
                {
                    _logger.LogError("Circular dependency detected in category hierarchy");
                    throw new BusinessRuleException("Circular dependency detected in category hierarchy");
                }
            }
            
            // Маппим обновления в существующую сущность
            _mapper.Map(updateDto, existingCategory);
            
            // Обновляем временную метку (автоматически в репозитории)
            
            // Сохраняем изменения
            await _categoryRepository.UpdateAsync(existingCategory, cancellationToken);
            
            _logger.LogInformation("Category with ID {CategoryId} updated successfully", id);
            
            var categoryDto = _mapper.Map<CategoryDto>(existingCategory);
            await FillCategoryCountsAsync(categoryDto, cancellationToken);
            
            return categoryDto;
        }

        public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting category with ID: {CategoryId}", id);
            
            // Проверяем, можно ли удалить категорию
            var checkResult = await CheckCategoryDeletionAsync(id, cancellationToken);
            if (!checkResult.CanBeDeleted)
            {
                _logger.LogError("Cannot delete category with ID {CategoryId}: {Message}", 
                    id, checkResult.Message);
                throw new BusinessRuleException(checkResult.Message);
            }
            
            var result = await _categoryRepository.DeleteByIdAsync(id, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Category with ID {CategoryId} deleted successfully", id);
            }
            
            return result;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting category path for category ID: {CategoryId}", categoryId);
            
            var categories = await _categoryRepository.GetCategoryPathAsync(categoryId, cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Заполняем дополнительные поля
            foreach (var categoryDto in categoryDtos)
            {
                await FillCategoryCountsAsync(categoryDto, cancellationToken);
            }
            
            return categoryDtos;
        }

        public async Task<CategoryDeletionCheckDto> CheckCategoryDeletionAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking if category with ID {CategoryId} can be deleted", categoryId);
            
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
            {
                return new CategoryDeletionCheckDto
                {
                    CanBeDeleted = false,
                    Message = $"Category with ID {categoryId} not found",
                    ComponentCount = 0,
                    ChildCategoryCount = 0
                };
            }
            
            // Проверяем наличие компонентов в категории
            var hasComponents = await _categoryRepository.HasComponentsAsync(categoryId, cancellationToken);
            
            // Проверяем наличие дочерних категорий
            var hasChildCategories = await _categoryRepository.HasChildCategoriesAsync(categoryId, cancellationToken);
            
            var componentCount = hasComponents ? 
                await _componentRepository.CountAsync(c => c.CategoryId == categoryId, cancellationToken) : 0;
            
            var childCategoryCount = hasChildCategories ? 
                await _categoryRepository.CountAsync(c => c.ParentCategoryId == categoryId, cancellationToken) : 0;
            
            var canBeDeleted = !hasComponents && !hasChildCategories;
            var message = canBeDeleted 
                ? "Category can be deleted"
                : $"Category cannot be deleted. " +
                  $"It contains {componentCount} component(s) and {childCategoryCount} child categor(ies).";
            
            return new CategoryDeletionCheckDto
            {
                CanBeDeleted = canBeDeleted,
                Message = message,
                ComponentCount = componentCount,
                ChildCategoryCount = childCategoryCount
            };
        }

        /// <summary>
        /// Заполняет количество компонентов и дочерних категорий для DTO
        /// </summary>
        private async Task FillCategoryCountsAsync(CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            categoryDto.ComponentCount = await _componentRepository.CountAsync(
                c => c.CategoryId == categoryDto.Id, 
                cancellationToken);
            
            categoryDto.ChildCategoryCount = await _categoryRepository.CountAsync(
                c => c.ParentCategoryId == categoryDto.Id, 
                cancellationToken);
            
            // Заполняем имя родительской категории
            if (categoryDto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(
                    categoryDto.ParentCategoryId.Value, 
                    cancellationToken);
                categoryDto.ParentCategoryName = parentCategory?.Name;
            }
        }

        /// <summary>
        /// Рекурсивно заполняет количество компонентов и дочерних категорий для иерархии
        /// </summary>
        private async Task FillCategoryHierarchyCountsAsync(CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            await FillCategoryCountsAsync(categoryDto, cancellationToken);
            
            // Рекурсивно обрабатываем дочерние категории
            foreach (var childCategory in categoryDto.ChildCategories)
            {
                await FillCategoryHierarchyCountsAsync(childCategory, cancellationToken);
            }
        }
    }
}
