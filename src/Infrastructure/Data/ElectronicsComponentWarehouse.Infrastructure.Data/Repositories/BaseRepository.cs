//Создаем базовый репозиторий
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using ElectronicsComponentWarehouse.Infrastructure.Data.ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Базовая реализация репозитория
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<BaseRepository<T>> _logger;

        protected BaseRepository(
            ApplicationDbContext context,
            ILogger<BaseRepository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all entities of type {Type}", typeof(T).Name);
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting entity of type {Type} with ID {Id}", typeof(T).Name, id);
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Finding entities of type {Type} with predicate", typeof(T).Name);
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Adding new entity of type {Type}", typeof(T).Name);
            
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Added entity of type {Type} with ID {Id}", 
                typeof(T).Name, GetEntityId(entity));
            
            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Updating entity of type {Type} with ID {Id}", 
                typeof(T).Name, GetEntityId(entity));
            
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated entity of type {Type} with ID {Id}", 
                typeof(T).Name, GetEntityId(entity));
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Deleting entity of type {Type} with ID {Id}", 
                typeof(T).Name, GetEntityId(entity));
            
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Deleted entity of type {Type} with ID {Id}", 
                typeof(T).Name, GetEntityId(entity));
        }

        public virtual async Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Entity of type {Type} with ID {Id} not found for deletion", 
                    typeof(T).Name, id);
                return false;
            }

            await DeleteAsync(entity, cancellationToken);
            return true;
        }

        public virtual async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking existence of entity of type {Type} with predicate", typeof(T).Name);
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Counting entities of type {Type}", typeof(T).Name);
            return await _dbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Counting entities of type {Type} with predicate", typeof(T).Name);
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Получить ID сущности (используется для логирования)
        /// </summary>
        private int GetEntityId(T entity)
        {
            var property = typeof(T).GetProperty("Id");
            if (property != null && property.PropertyType == typeof(int))
            {
                return (int)property.GetValue(entity)!;
            }
            return -1;
        }
    }
}
