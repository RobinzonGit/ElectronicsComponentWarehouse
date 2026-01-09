// Создаем сущность Category (категория компонентов)
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsComponentWarehouse.Domain.Entities
{
    /// <summary>
    /// Категория электронных компонентов (иерархическая структура)
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Уникальный идентификатор категории
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название категории (например, "Microcontrollers", "Resistors")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание категории (опционально)
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Идентификатор родительской категории (для иерархии)
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Родительская категория (навигационное свойство)
        /// </summary>
        [ForeignKey(nameof(ParentCategoryId))]
        public virtual Category? ParentCategory { get; set; }

        /// <summary>
        /// Дочерние категории (навигационное свойство)
        /// </summary>
        public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();

        /// <summary>
        /// Компоненты, принадлежащие этой категории (навигационное свойство)
        /// </summary>
        public virtual ICollection<Component> Components { get; set; } = new List<Component>();

        /// <summary>
        /// Дата и время создания категории
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата и время последнего обновления категории
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
