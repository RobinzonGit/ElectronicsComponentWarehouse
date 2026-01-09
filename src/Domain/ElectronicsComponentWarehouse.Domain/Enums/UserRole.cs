// Создаем перечисление для ролей пользователей
namespace ElectronicsComponentWarehouse.Domain.Enums
{
    /// <summary>
    /// Роли пользователей в системе
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Обычный пользователь (только чтение + обновление количества и ссылки)
        /// </summary>
        User = 0,
        
        /// <summary>
        /// Администратор (полный CRUD доступ)
        /// </summary>
        Admin = 1
    }
}
