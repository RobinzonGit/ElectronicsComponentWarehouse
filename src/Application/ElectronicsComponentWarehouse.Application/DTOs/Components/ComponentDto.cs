//DTO для компонентов
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Components
{
    /// <summary>
    /// DTO для отображения информации о компоненте
    /// </summary>
    public class ComponentDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
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
        
        public string? CategoryName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Флаг, указывающий на низкий запас (ниже минимального уровня)
        /// </summary>
        public bool IsLowStock => StockQuantity <= MinimumStockLevel;
        
        /// <summary>
        /// Общая стоимость компонентов на складе (количество * цена)
        /// </summary>
        public decimal? TotalValue => UnitPrice.HasValue ? UnitPrice.Value * StockQuantity : null;
    }
}
