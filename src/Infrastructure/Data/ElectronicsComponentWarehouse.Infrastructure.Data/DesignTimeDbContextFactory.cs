using ElectronicsComponentWarehouse.Infrastructure.Data.ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using ElectronicsComponentWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ElectronicsComponentWarehouse.Infrastructure.Data
{
    /// <summary>
    /// Фабрика для создания ApplicationDbContext во время миграций
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Получаем конфигурацию из appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                //.AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Если строка подключения не найдена, используем значение по умолчанию
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=ElectronicsComponentWarehouseDB_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}