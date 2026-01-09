//Создаем конфигурацию для сущности Component
using ElectronicsComponentWarehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности Component для Entity Framework Core
    /// </summary>
    public class ComponentConfiguration : IEntityTypeConfiguration<Component>
    {
        public void Configure(EntityTypeBuilder<Component> builder)
        {
            builder.ToTable("Components");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.StorageCellNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Manufacturer)
                .HasMaxLength(100);

            builder.Property(c => c.ModelNumber)
                .HasMaxLength(100);

            builder.Property(c => c.DatasheetLink)
                .HasMaxLength(500);

            builder.Property(c => c.MinimumStockLevel)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(c => c.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.LastUpdated)
                .IsRequired();

            // Связь с категорией
            builder.HasOne(c => c.Category)
                .WithMany(cat => cat.Components)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять компоненты при удалении категории

            // Индексы
            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.CategoryId);
            builder.HasIndex(c => c.StorageCellNumber).IsUnique();
            builder.HasIndex(c => c.Manufacturer);
            builder.HasIndex(c => c.StockQuantity);

            // Проверочное ограничение для количества
            builder.HasCheckConstraint("CK_Component_StockQuantity", "[StockQuantity] >= 0");
            builder.HasCheckConstraint("CK_Component_MinimumStockLevel", "[MinimumStockLevel] >= 0");

            // Заполнение начальными данными
            builder.HasData(
                new Component
                {
                    Id = 1,
                    Name = "Arduino Uno R3",
                    Description = "Микроконтроллерная плата на базе ATmega328P",
                    StockQuantity = 25,
                    StorageCellNumber = "A-01-01",
                    Manufacturer = "Arduino",
                    ModelNumber = "A000066",
                    DatasheetLink = "https://docs.arduino.cc/resources/datasheets/A000066-datasheet.pdf",
                    MinimumStockLevel = 5,
                    UnitPrice = 22.00m,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                },
                new Component
                {
                    Id = 2,
                    Name = "ESP32 DevKit",
                    Description = "Модуль WiFi+Bluetooth на ESP32",
                    StockQuantity = 18,
                    StorageCellNumber = "A-01-02",
                    Manufacturer = "Espressif",
                    ModelNumber = "ESP32-WROOM-32",
                    DatasheetLink = "https://www.espressif.com/sites/default/files/documentation/esp32-wroom-32_datasheet_en.pdf",
                    MinimumStockLevel = 8,
                    UnitPrice = 8.50m,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    LastUpdated = DateTime.UtcNow
                },
                new Component
                {
                    Id = 3,
                    Name = "Resistor 10kΩ 1/4W",
                    Description = "Резистор 10 кОм, 0.25 Вт, 5%",
                    StockQuantity = 1000,
                    StorageCellNumber = "B-02-01",
                    Manufacturer = "Yageo",
                    ModelNumber = "CFR-25JB-10K",
                    MinimumStockLevel = 200,
                    UnitPrice = 0.02m,
                    CategoryId = 5,
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    LastUpdated = DateTime.UtcNow.AddDays(-5)
                }
            );
        }
    }
}
