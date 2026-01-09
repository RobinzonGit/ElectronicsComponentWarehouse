//Создаем конфигурацию для сущности User
using ElectronicsComponentWarehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности User для Entity Framework Core
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.PasswordSalt)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.LastLoginAt)
                .IsRequired(false);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Индексы
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Role);

            // Заполнение начальными данными (тестовые пользователи)
            // В реальном приложении пароли должны хешироваться с солью
            builder.HasData(
                CreateUser(
                    id: 1,
                    username: "admin",
                    password: "Admin123!", // В реальном приложении будет хэш
                    email: "admin@warehouse.local",
                    fullName: "Администратор Системы",
                    role: Domain.Enums.UserRole.Admin
                ),
                CreateUser(
                    id: 2,
                    username: "user",
                    password: "User123!", // В реальном приложении будет хэш
                    email: "user@warehouse.local",
                    fullName: "Обычный Пользователь",
                    role: Domain.Enums.UserRole.User
                ),
                CreateUser(
                    id: 3,
                    username: "manager",
                    password: "Manager123!", // В реальном приложении будет хэш
                    email: "manager@warehouse.local",
                    fullName: "Менеджер Склада",
                    role: Domain.Enums.UserRole.User
                )
            );
        }

        private static User CreateUser(int id, string username, string password, string email, 
            string fullName, Domain.Enums.UserRole role)
        {
            // В реальном приложении здесь должна быть логика хэширования пароля
            // Для примера используем простую конвертацию в байты
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var salt = Encoding.UTF8.GetBytes($"salt_for_{username}");
            
            return new User
            {
                Id = id,
                Username = username,
                PasswordHash = passwordBytes,
                PasswordSalt = salt,
                Email = email,
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                LastLoginAt = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            };
        }
    }
}
