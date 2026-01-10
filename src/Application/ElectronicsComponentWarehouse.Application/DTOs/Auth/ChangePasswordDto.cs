//DTO для изменения пароля
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Auth
{
    /// <summary>
    /// DTO для изменения пароля
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
