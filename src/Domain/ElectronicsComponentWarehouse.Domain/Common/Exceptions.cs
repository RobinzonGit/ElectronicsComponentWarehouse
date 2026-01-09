//Создаем класс для кастомных исключений
namespace ElectronicsComponentWarehouse.Domain.Common
{
    /// <summary>
    /// Базовое исключение для домена
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Исключение при ненайденной сущности
    /// </summary>
    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityName, int id) 
            : base($"{entityName} with ID {id} was not found.")
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Исключение при нарушении бизнес-правил
    /// </summary>
    public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Исключение при недостаточном количестве на складе
    /// </summary>
    public class InsufficientStockException : BusinessRuleException
    {
        public InsufficientStockException(string componentName, int requested, int available)
            : base($"Cannot process request for {componentName}. " +
                   $"Requested: {requested}, Available: {available}")
        {
        }
    }
}
