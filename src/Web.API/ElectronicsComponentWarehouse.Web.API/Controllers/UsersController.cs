//Создаем контроллер пользователей
using ElectronicsComponentWarehouse.Application.DTOs.Users;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Получение всех пользователей
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список пользователей</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all users");

            try
            {
                var users = await _userService.GetAllUsersAsync(cancellationToken);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Получение пользователя по ID
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Пользователь</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", id);

            try
            {
                var user = await _userService.GetUserByIdAsync(id, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while retrieving user with ID {id}" });
            }
        }

        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="createDto">Данные для создания пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданный пользователь</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserDto createDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new user: {Username}", createDto.Username);

            try
            {
                var createdUser = await _userService.CreateUserAsync(createDto, cancellationToken);
                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = createdUser.Id },
                    createdUser);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while creating user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", createDto.Username);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the user" });
            }
        }

        /// <summary>
        /// Обновление пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="updateDto">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный пользователь</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUser(
            int id,
            [FromBody] UpdateUserDto updateDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating user ID: {UserId}", id);

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id, updateDto, cancellationToken);
                return Ok(updatedUser);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while updating user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while updating user with ID {id}" });
            }
        }

        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting user ID: {UserId}", id);

            // Нельзя удалить самого себя
            var currentUserId = GetCurrentUserId();
            if (currentUserId == id)
            {
                return BadRequest(new { message = "You cannot delete your own account" });
            }

            try
            {
                var deleted = await _userService.DeleteUserAsync(id, cancellationToken);
                if (!deleted)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while deleting user with ID {id}" });
            }
        }

        /// <summary>
        /// Обновление роли пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="roleDto">Новая роль</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный пользователь</returns>
        [HttpPut("{id:int}/role")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUserRole(
            int id,
            [FromBody] UpdateUserRoleDto roleDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating role for user ID: {UserId} to {Role}", id, roleDto.Role);

            try
            {
                var updatedUser = await _userService.UpdateUserRoleAsync(id, roleDto.Role, cancellationToken);
                return Ok(updatedUser);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid role for user ID: {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while updating role for user with ID {id}" });
            }
        }

        /// <summary>
        /// Активация/деактивация пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="statusDto">Статус активации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный пользователь</returns>
        [HttpPut("{id:int}/status")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SetUserStatus(
            int id,
            [FromBody] SetUserStatusDto statusDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting status for user ID: {UserId} to {IsActive}", id, statusDto.IsActive);

            // Нельзя деактивировать самого себя
            var currentUserId = GetCurrentUserId();
            if (currentUserId == id && !statusDto.IsActive)
            {
                return BadRequest(new { message = "You cannot deactivate your own account" });
            }

            try
            {
                var updatedUser = await _userService.SetUserActiveStatusAsync(id, statusDto.IsActive, cancellationToken);
                return Ok(updatedUser);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting status for user ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while setting status for user with ID {id}" });
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
    }

    /// <summary>
    /// DTO для обновления роли пользователя
    /// </summary>
    public class UpdateUserRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для установки статуса пользователя
    /// </summary>
    public class SetUserStatusDto
    {
        public bool IsActive { get; set; }
    }
}