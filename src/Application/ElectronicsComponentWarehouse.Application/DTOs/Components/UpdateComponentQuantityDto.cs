//DTO для обновления только количества компонента (для пользовательского режима)
using System.ComponentModel.DataAnnotations;

namespace ElectronicsComponentWarehouse.Application.DTOs.Components
{
    /// <summary>
    /// DTO для обновления количества компонента (используется в режиме пользователя)
    /// </summary>
    public class UpdateComponentQuantityDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        
        [StringLength(500)]
        [Url]
        public string? DatasheetLink { get; set; }
    }
}
