//DTO для пользователей
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Users
{
    /// <summary>
    /// DTO для отображения информации о пользователе
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; }
    }
}
