//Создаем конфигурацию для сущности Category
using ElectronicsComponentWarehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности Category для Entity Framework Core
    /// </summary>
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired();

            // Самореференциальная связь для иерархии категорий
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять дочерние категории при удалении родителя

            // Индексы
            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasIndex(c => c.ParentCategoryId);

            // Заполнение начальными данными
            builder.HasData(
                new Category { Id = 1, Name = "Microcontrollers", Description = "Микроконтроллеры и микропроцессоры" },
                new Category { Id = 2, Name = "Microchips", Description = "Микросхемы и интегральные схемы" },
                new Category { Id = 3, Name = "Mosfets", Description = "Полевые транзисторы (MOSFET)" },
                new Category { Id = 4, Name = "Capacitors", Description = "Конденсаторы" },
                new Category { Id = 5, Name = "Resistors", Description = "Резисторы" },
                new Category { Id = 6, Name = "Diodes", Description = "Диоды и стабилитроны" },
                new Category { Id = 7, Name = "Transistors", Description = "Биполярные транзисторы" },
                new Category { Id = 8, Name = "Connectors", Description = "Разъемы и коннекторы" },
                new Category { Id = 9, Name = "Sensors", Description = "Датчики и сенсоры" },
                new Category { Id = 10, Name = "Displays", Description = "Дисплеи и индикаторы" }
            );
        }
    }
}
