using ElectronicsComponentWarehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.ElectronicsComponentWarehouse.Infrastructure.Data.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Конструктор без параметров для миграций EF Core
        protected ApplicationDbContext()
        {
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
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Этот код выполняется только при создании миграций
                // В продакшене строка подключения берется из конфигурации
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ElectronicsComponentWarehouseDB_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false");
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Автоматическое обновление временных меток
            UpdateTimestamps();

            return await base.SaveChangesAsync(cancellationToken);
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
            }
        }
    }
}