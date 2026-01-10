//DTO для создания компонента
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Components
{
    /// <summary>
    /// DTO для создания нового компонента
    /// </summary>
    public class CreateComponentDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string StorageCellNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? ModelNumber { get; set; }

        [Url]
        [StringLength(500)]
        public string? DatasheetLink { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; } = 10;

        [Range(0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}