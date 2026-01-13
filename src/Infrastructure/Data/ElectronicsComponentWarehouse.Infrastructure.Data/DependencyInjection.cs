using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using ElectronicsComponentWarehouse.Infrastructure.Data.ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using ElectronicsComponentWarehouse.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Infrastructure.Data
{
    /// <summary>
    /// Класс для регистрации зависимостей слоя Infrastructure.Data
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы слоя Infrastructure.Data в DI-контейнер
        /// </summary>
        public static IServiceCollection AddInfrastructureData(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Регистрируем DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Connection string 'DefaultConnection' not found in configuration.");
                }

                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });

                // Включаем подробное логирование для разработки
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Регистрируем репозитории
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        /// <summary>
        /// Настраивает миграции базы данных
        /// </summary>
        public static async Task<IServiceProvider> MigrateDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while migrating the database");
                throw;
            }

            return serviceProvider;
        }
    }
}