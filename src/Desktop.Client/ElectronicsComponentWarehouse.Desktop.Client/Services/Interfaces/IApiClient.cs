using System.Net.Http.Headers;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces
{
    /// <summary>
    /// Клиент для работы с API
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Базовый URL API
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Выполнить GET запрос
        /// </summary>
        Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполнить POST запрос
        /// </summary>
        Task<T?> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполнить PUT запрос
        /// </summary>
        Task<T?> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполнить PATCH запрос
        /// </summary>
        Task<T?> PatchAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполнить DELETE запрос
        /// </summary>
        Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Установить токен авторизации
        /// </summary>
        void SetAuthorizationToken(string token);

        /// <summary>
        /// Очистить токен авторизации
        /// </summary>
        void ClearAuthorizationToken();

        /// <summary>
        /// Проверить подключение к API
        /// </summary>
        Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
    }
}