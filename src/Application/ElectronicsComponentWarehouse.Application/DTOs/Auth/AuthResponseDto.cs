//DTO для ответа аутентификации
namespace ElectronicsComponentWarehouse.Application.DTOs.Auth
{
    /// <summary>
    /// DTO для ответа после успешной аутентификации
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }

    /// <summary>
    /// Информация о пользователе для ответа
    /// </summary>
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
