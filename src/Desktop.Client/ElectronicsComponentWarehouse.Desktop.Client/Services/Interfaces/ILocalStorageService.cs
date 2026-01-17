namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces
{
    /// <summary>
    /// Сервис локального хранилища
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        /// Сохранить значение
        /// </summary>
        Task SaveAsync<T>(string key, T value);

        /// <summary>
        /// Загрузить значение
        /// </summary>
        Task<T?> LoadAsync<T>(string key);

        /// <summary>
        /// Удалить значение
        /// </summary>
        Task DeleteAsync(string key);

        /// <summary>
        /// Проверить наличие ключа
        /// </summary>
        Task<bool> ContainsKeyAsync(string key);

        /// <summary>
        /// Очистить хранилище
        /// </summary>
        Task ClearAsync();
    }
}