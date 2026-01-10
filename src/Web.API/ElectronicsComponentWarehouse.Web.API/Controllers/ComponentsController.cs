//Создаем контроллер компонентов
using ElectronicsComponentWarehouse.Application.DTOs.Components;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Controllers
{
    /// <summary>
    /// Контроллер для управления электронными компонентами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Components")]
    [Authorize]
    public class ComponentsController : ControllerBase
    {
        private readonly IComponentService _componentService;
        private readonly ILogger<ComponentsController> _logger;

        public ComponentsController(
            IComponentService componentService,
            ILogger<ComponentsController> logger)
        {
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Получение всех компонентов
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список компонентов</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ComponentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllComponents(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all components");

            try
            {
                var components = await _componentService.GetAllComponentsAsync(cancellationToken);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all components");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving components" });
            }
        }

        /// <summary>
        /// Получение компонента по ID
        /// </summary>
        /// <param name="id">ID компонента</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Компонент</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ComponentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetComponentById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting component by ID: {ComponentId}", id);

            try
            {
                var component = await _componentService.GetComponentByIdAsync(id, cancellationToken);
                if (component == null)
                {
                    return NotFound(new { message = $"Component with ID {id} not found" });
                }
                return Ok(component);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting component by ID: {ComponentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while retrieving component with ID {id}" });
            }
        }

        /// <summary>
        /// Получение компонентов по категории
        /// </summary>
        /// <param name="categoryId">ID категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список компонентов в категории</returns>
        [HttpGet("category/{categoryId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ComponentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetComponentsByCategory(int categoryId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting components by category ID: {CategoryId}", categoryId);

            try
            {
                var components = await _componentService.GetComponentsByCategoryIdAsync(categoryId, cancellationToken);
                return Ok(components);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", categoryId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting components by category ID: {CategoryId}", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while retrieving components for category {categoryId}" });
            }
        }

        /// <summary>
        /// Создание нового компонента
        /// </summary>
        /// <param name="createDto">Данные для создания компонента</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданный компонент</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ComponentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateComponent(
            [FromBody] CreateComponentDto createDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new component: {ComponentName}", createDto.Name);

            try
            {
                var createdComponent = await _componentService.CreateComponentAsync(createDto, cancellationToken);
                return CreatedAtAction(
                    nameof(GetComponentById),
                    new { id = createdComponent.Id },
                    createdComponent);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entity not found while creating component");
                return BadRequest(new { message = ex.Message });
            }
            catch (Domain.Common.BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating component");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while creating component");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating component: {ComponentName}", createDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the component" });
            }
        }

        /// <summary>
        /// Обновление компонента
        /// </summary>
        /// <param name="id">ID компонента</param>
        /// <param name="updateDto">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный компонент</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ComponentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateComponent(
            int id,
            [FromBody] UpdateComponentDto updateDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating component ID: {ComponentId}", id);

            try
            {
                var updatedComponent = await _componentService.UpdateComponentAsync(id, updateDto, cancellationToken);
                return Ok(updatedComponent);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Component not found: {ComponentId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Domain.Common.BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while updating component");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating component ID: {ComponentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while updating component with ID {id}" });
            }
        }

        /// <summary>
        /// Обновление количества компонента (доступно для пользователей)
        /// </summary>
        /// <param name="id">ID компонента</param>
        /// <param name="updateDto">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный компонент</returns>
        [HttpPatch("{id:int}/quantity")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ComponentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateComponentQuantity(
            int id,
            [FromBody] UpdateComponentQuantityDto updateDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating quantity for component ID: {ComponentId}", id);

            try
            {
                var updatedComponent = await _componentService.UpdateComponentQuantityAsync(id, updateDto, cancellationToken);
                return Ok(updatedComponent);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Component not found: {ComponentId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for component ID: {ComponentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while updating quantity for component with ID {id}" });
            }
        }

        /// <summary>
        /// Удаление компонента
        /// </summary>
        /// <param name="id">ID компонента</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteComponent(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting component ID: {ComponentId}", id);

            try
            {
                var deleted = await _componentService.DeleteComponentAsync(id, cancellationToken);
                if (!deleted)
                {
                    return NotFound(new { message = $"Component with ID {id} not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting component ID: {ComponentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while deleting component with ID {id}" });
            }
        }

        /// <summary>
        /// Поиск компонентов
        /// </summary>
        /// <param name="searchTerm">Поисковый запрос</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Найденные компоненты</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ComponentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchComponents(
            [FromQuery] string searchTerm,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching components with term: {SearchTerm}", searchTerm);

            try
            {
                var components = await _componentService.SearchComponentsAsync(searchTerm, cancellationToken);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching components with term: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while searching components" });
            }
        }

        /// <summary>
        /// Получение компонентов с низким запасом
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Компоненты с низким запасом</returns>
        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<ComponentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLowStockComponents(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting low stock components");

            try
            {
                var components = await _componentService.GetLowStockComponentsAsync(cancellationToken);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock components");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving low stock components" });
            }
        }

        /// <summary>
        /// Получение статистики по компонентам
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статистика</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ComponentStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting components statistics");

            try
            {
                var statistics = await _componentService.GetComponentsStatisticsAsync(cancellationToken);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting components statistics");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics" });
            }
        }
    }
}