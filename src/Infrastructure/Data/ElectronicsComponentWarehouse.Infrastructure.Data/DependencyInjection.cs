//Создаем класс для регистрации сервисов в DI-контейнере
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using ElectronicsComponentWarehouse.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

                options.LogTo(Console.WriteLine, LogLevel.Information);
            });

            // Регистрируем репозитории
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Регистрируем фабрику DbContext для использования в фоновых задачах
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

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
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

                logger.LogInformation("Checking for pending migrations...");

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("Database is up to date");
                }

                // Проверяем подключение к базе данных
                var canConnect = await context.CanConnectAsync();
                if (canConnect)
                {
                    logger.LogInformation("Database connection test: SUCCESS");
                }
                else
                {
                    logger.LogWarning("Database connection test: FAILED");
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