//Создаем контроллер категорий
using ElectronicsComponentWarehouse.Application.DTOs.Categories;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Controllers
{
    /// <summary>
    /// Контроллер для управления категориями компонентов
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Получение всех категорий
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список категорий</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all categories");

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving categories" });
            }
        }

        /// <summary>
        /// Получение категории по ID
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Категория</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoryById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting category by ID: {CategoryId}", id);

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    return NotFound(new { message = $"Category with ID {id} not found" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while retrieving category with ID {id}" });
            }
        }

        /// <summary>
        /// Получение корневых категорий (без родителя)
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Корневые категории</returns>
        [HttpGet("roots")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRootCategories(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting root categories");

            try
            {
                var categories = await _categoryService.GetRootCategoriesAsync(cancellationToken);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting root categories");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving root categories" });
            }
        }

        /// <summary>
        /// Получение дочерних категорий
        /// </summary>
        /// <param name="parentCategoryId">ID родительской категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Дочерние категории</returns>
        [HttpGet("parent/{parentCategoryId:int}")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetChildCategories(
            int parentCategoryId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting child categories for parent ID: {ParentCategoryId}", parentCategoryId);

            try
            {
                var categories = await _categoryService.GetChildCategoriesAsync(parentCategoryId, cancellationToken);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting child categories for parent ID: {ParentCategoryId}", parentCategoryId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while retrieving child categories for parent {parentCategoryId}" });
            }
        }

        /// <summary>
        /// Получение полной иерархии категорий
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Иерархия категорий</returns>
        [HttpGet("hierarchy")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoryHierarchy(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting category hierarchy");

            try
            {
                var hierarchy = await _categoryService.GetCategoryHierarchyAsync(cancellationToken);
                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category hierarchy");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving category hierarchy" });
            }
        }

        /// <summary>
        /// Создание новой категории
        /// </summary>
        /// <param name="createDto">Данные для создания категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданная категория</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CreateCategoryDto createDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new category: {CategoryName}", createDto.Name);

            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(createDto, cancellationToken);
                return CreatedAtAction(
                    nameof(GetCategoryById),
                    new { id = createdCategory.Id },
                    createdCategory);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entity not found while creating category");
                return BadRequest(new { message = ex.Message });
            }
            catch (Domain.Common.BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating category");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", createDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the category" });
            }
        }

        /// <summary>
        /// Обновление категории
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <param name="updateDto">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленная категория</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] UpdateCategoryDto updateDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating category ID: {CategoryId}", id);

            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateDto, cancellationToken);
                return Ok(updatedCategory);
            }
            catch (Domain.Common.EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Domain.Common.BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while updating category");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID: {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while updating category with ID {id}" });
            }
        }

        /// <summary>
        /// Удаление категории
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting category ID: {CategoryId}", id);

            try
            {
                // Проверяем, можно ли удалить категорию
                var checkResult = await _categoryService.CheckCategoryDeletionAsync(id, cancellationToken);
                if (!checkResult.CanBeDeleted)
                {
                    return BadRequest(new
                    {
                        message = checkResult.Message,
                        details = new
                        {
                            componentCount = checkResult.ComponentCount,
                            childCategoryCount = checkResult.ChildCategoryCount
                        }
                    });
                }

                var deleted = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
                if (!deleted)
                {
                    return NotFound(new { message = $"Category with ID {id} not found" });
                }
                return NoContent();
            }
            catch (Domain.Common.BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Cannot delete category ID: {CategoryId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category ID: {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while deleting category with ID {id}" });
            }
        }

        /// <summary>
        /// Проверка возможности удаления категории
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат проверки</returns>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoryDeletionCheckDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CanDeleteCategory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking if category ID {CategoryId} can be deleted", id);

            try
            {
                var checkResult = await _categoryService.CheckCategoryDeletionAsync(id, cancellationToken);
                return Ok(checkResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category ID {CategoryId} can be deleted", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while checking category deletion for ID {id}" });
            }
        }

        /// <summary>
        /// Получение пути категории (все родительские категории)
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Путь категории</returns>
        [HttpGet("{id:int}/path")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoryPath(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting category path for category ID: {CategoryId}", id);

            try
            {
                var path = await _categoryService.GetCategoryPathAsync(id, cancellationToken);
                return Ok(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category path for category ID: {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred while retrieving category path for ID {id}" });
            }
        }
    }
}