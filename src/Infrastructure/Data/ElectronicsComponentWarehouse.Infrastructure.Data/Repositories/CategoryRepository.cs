//Создаем репозиторий для Category
using ElectronicsComponentWarehouse.Domain.Entities;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с категориями
    /// </summary>
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(
            ApplicationDbContext context,
            ILogger<CategoryRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting root categories");
            return await _dbSet
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetChildCategoriesAsync(
            int parentCategoryId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting child categories for parent ID {ParentId}", parentCategoryId);
            return await _dbSet
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting full category hierarchy");
            
            var categories = await _dbSet
                .Include(c => c.ChildCategories)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Рекурсивно загружаем вложенные категории
            foreach (var category in categories)
            {
                await LoadChildCategoriesRecursiveAsync(category, cancellationToken);
            }

            return categories;
        }

        private async Task LoadChildCategoriesRecursiveAsync(Category category, CancellationToken cancellationToken)
        {
            var childCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == category.Id)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var child in childCategories)
            {
                category.ChildCategories.Add(child);
                await LoadChildCategoriesRecursiveAsync(child, cancellationToken);
            }
        }

        public async Task<bool> HasChildCategoriesAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if category ID {CategoryId} has child categories", categoryId);
            return await _dbSet.AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
        }

        public async Task<bool> HasComponentsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if category ID {CategoryId} has components", categoryId);
            return await _context.Components.AnyAsync(c => c.CategoryId == categoryId, cancellationToken);
        }

        public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting category by name: {Name}", name);
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting category path for category ID {CategoryId}", categoryId);
            
            var path = new List<Category>();
            var currentCategory = await _dbSet.FindAsync(new object[] { categoryId }, cancellationToken);
            
            if (currentCategory == null)
                return path;

            path.Add(currentCategory);

            // Поднимаемся вверх по иерархии
            while (currentCategory.ParentCategoryId.HasValue)
            {
                currentCategory = await _dbSet.FindAsync(
                    new object[] { currentCategory.ParentCategoryId.Value }, 
                    cancellationToken);
                
                if (currentCategory == null)
                    break;
                    
                path.Insert(0, currentCategory);
            }

            return path;
        }

        // Переопределяем метод GetAllAsync для включения связанных данных
        public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all categories with parent information");
            return await _dbSet
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // Переопределяем метод GetByIdAsync для включения связанных данных
        public override async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting category by ID {Id} with parent information", id);
            return await _dbSet
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
    }
}
