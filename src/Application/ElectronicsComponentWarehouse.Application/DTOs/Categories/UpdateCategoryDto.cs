//DTO для обновления категории
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Categories
{
    /// <summary>
    /// DTO для обновления существующей категории
    /// </summary>
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? ParentCategoryId { get; set; }
    }
}
