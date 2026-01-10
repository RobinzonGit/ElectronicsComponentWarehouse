//DTO для категорий
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Categories
{
    /// <summary>
    /// DTO для отображения информации о категории
    /// </summary>
    public class CategoryDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? ParentCategoryId { get; set; }
        
        public string? ParentCategoryName { get; set; }
        
        public int ComponentCount { get; set; }
        
        public int ChildCategoryCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
    /// Список дочерних категорий (для иерархического отображения)
    /// </summary>
        public List<CategoryDto> ChildCategories { get; set; } = new();
    }
}
