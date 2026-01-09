//Создаем сущность User (пользователь системы)
using ElectronicsComponentWarehouse.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Domain.Entities
{
    /// <summary>
    /// Пользователь системы для аутентификации и авторизации
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя для входа в систему
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Хэш пароля (никогда не храним пароль в открытом виде!)
        /// </summary>
        [Required]
        [MaxLength(256)]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Соль для пароля (дополнительная защита)
        /// </summary>
        [Required]
        [MaxLength(256)]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Полное имя пользователя
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Роль пользователя в системе
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Дата и время создания учетной записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата и время последнего входа в систему
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Активна ли учетная запись
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
