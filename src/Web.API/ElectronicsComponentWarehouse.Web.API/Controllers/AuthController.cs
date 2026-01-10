//Создаем контроллер аутентификации
using ElectronicsComponentWarehouse.Application.DTOs.Auth;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Controllers
{
    /// <summary>
    /// Контроллер для аутентификации и авторизации
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Вход в систему
        /// </summary>
        /// <param name="loginDto">Данные для входа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Токен доступа и информация о пользователе</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto loginDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login attempt for user: {Username}", loginDto.Username);

            try
            {
                var authResponse = await _authService.LoginAsync(loginDto, cancellationToken);
                return Ok(authResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed for user {Username}: {Message}",
                    loginDto.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return BadRequest(new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="registerDto">Данные для регистрации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Токен доступа и информация о пользователе</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(
            [FromBody] Application.DTOs.Users.CreateUserDto registerDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Registration attempt for user: {Username}", registerDto.Username);

            try
            {
                var authResponse = await _authService.RegisterAsync(registerDto, cancellationToken);
                return CreatedAtAction(nameof(Login), authResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
                return BadRequest(new { message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Изменение пароля пользователя
        /// </summary>
        /// <param name="changePasswordDto">Данные для изменения пароля</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto changePasswordDto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            _logger.LogInformation("Password change attempt for user ID: {UserId}", userId);

            try
            {
                var success = await _authService.ChangePasswordAsync(userId.Value, changePasswordDto, cancellationToken);
                if (success)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                return BadRequest(new { message = "Failed to change password" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Password change failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Password change failed: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user ID {UserId}", userId);
                return BadRequest(new { message = "An error occurred while changing password" });
            }
        }

        /// <summary>
        /// Проверка валидности токена
        /// </summary>
        /// <returns>Информация о пользователе</returns>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateToken(CancellationToken cancellationToken)
        {
            var token = GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token not provided" });
            }

            try
            {
                var userInfo = await _authService.GetUserFromTokenAsync(token, cancellationToken);
                if (userInfo == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Unauthorized(new { message = "Token validation failed" });
            }
        }

        /// <summary>
        /// Получение информации о текущем пользователе
        /// </summary>
        /// <returns>Информация о текущем пользователе</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var token = GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token not provided" });
            }

            try
            {
                var userInfo = await _authService.GetUserFromTokenAsync(token, cancellationToken);
                if (userInfo == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return Unauthorized(new { message = "Failed to get user information" });
            }
        }

        /// <summary>
        /// Получение ID текущего пользователя из токена
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Получение токена из заголовка Authorization
        /// </summary>
        private string? GetTokenFromHeader()
        {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }
    }
}