//DTO для создания категории
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Categories
{
    /// <summary>
    /// DTO для создания новой категории
    /// </summary>
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? ParentCategoryId { get; set; }
    }
}
