//Создаем базовый интерфейс для сущностей с общими полями
namespace ElectronicsComponentWarehouse.Domain.Entities
{
    /// <summary>
    /// Базовый класс для всех сущностей с общими свойствами
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Уникальный идентификатор сущности
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Дата и время создания сущности
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
