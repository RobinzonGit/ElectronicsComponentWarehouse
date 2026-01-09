//Создаем DbContext
using ElectronicsComponentWarehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        // DbSet для каждой сущности
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Component> Components { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Применяем все конфигурации из текущей сборки
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            _logger.LogInformation("Database model configured successfully");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Автоматическое обновление временных меток
            UpdateTimestamps();
            
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Saved {Count} changes to database", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        /// <summary>
        /// Автоматически обновляет поля CreatedAt и UpdatedAt/LastUpdated
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && 
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            var utcNow = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = utcNow;
                }

                // Для Component обновляем LastUpdated
                if (entry.Entity is Component component)
                {
                    component.LastUpdated = utcNow;
                }
                // Для Category обновляем UpdatedAt
                else if (entry.Entity is Category category)
                {
                    category.UpdatedAt = utcNow;
                }
                // Для User не обновляем временные метки при каждом изменении
            }
        }

        /// <summary>
        /// Проверка подключения к базе данных
        /// </summary>
        public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Database.CanConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return false;
            }
        }
    }
}
