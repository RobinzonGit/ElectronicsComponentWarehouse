//  Создаем сущность Component (электронный компонент)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsComponentWarehouse.Domain.Entities
{
    /// <summary>
    /// Электронный компонент для хранения на складе
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Уникальный идентификатор компонента
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название компонента (например, "Arduino Uno", "Resistor 10kOhm")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание компонента (характеристики, особенности)
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Количество компонентов на складе
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Номер ячейки хранения на складе
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string StorageCellNumber { get; set; } = string.Empty;

        /// <summary>
        /// Производитель компонента
        /// </summary>
        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Модель/парт-номер компонента
        /// </summary>
        [MaxLength(100)]
        public string? ModelNumber { get; set; }

        /// <summary>
        /// Ссылка на документацию (Datasheet)
        /// </summary>
        [MaxLength(500)]
        [Url]
        public string? DatasheetLink { get; set; }

        /// <summary>
        /// Минимальное допустимое количество (для оповещений)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// Цена за единицу (в базовой валюте)
        /// </summary>
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Идентификатор категории, к которой принадлежит компонент
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Категория компонента (навигационное свойство)
        /// </summary>
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;

        /// <summary>
        /// Дата и время создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата и время последнего обновления записи (автоматически обновляется)
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
