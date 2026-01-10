//DTO для аутентификации
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Auth
{
    /// <summary>
    /// DTO для входа в систему
    /// </summary>
    public class LoginDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}