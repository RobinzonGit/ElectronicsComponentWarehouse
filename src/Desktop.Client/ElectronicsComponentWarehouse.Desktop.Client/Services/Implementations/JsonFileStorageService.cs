using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations
{
    /// <summary>
    /// Реализация локального хранилища на основе JSON файлов
    /// </summary>
    public class JsonFileStorageService : ILocalStorageService
    {
        private readonly ILogger<JsonFileStorageService> _logger;
        private readonly string _storagePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFileStorageService(ILogger<JsonFileStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Путь к папке хранения (в AppData)
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _storagePath = Path.Combine(appDataPath, "ElectronicsComponentWarehouse", "Storage");

            // Создаем папку, если не существует
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
                _logger.LogInformation("Создана папка хранилища: {StoragePath}", _storagePath);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public async Task SaveAsync<T>(string key, T value)
        {
            try
            {
                var filePath = GetFilePath(key);
                var json = JsonSerializer.Serialize(value, _jsonOptions);

                await File.WriteAllTextAsync(filePath, json);
                _logger.LogDebug("Сохранено значение по ключу: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении значения по ключу: {Key}", key);
                throw;
            }
        }

        public async Task<T?> LoadAsync<T>(string key)
        {
            try
            {
                var filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                {
                    _logger.LogDebug("Файл не найден для ключа: {Key}", key);
                    return default;
                }

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке значения по ключу: {Key}", key);
                return default;
            }
        }

        public Task DeleteAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogDebug("Удалено значение по ключу: {Key}", key);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении значения по ключу: {Key}", key);
                throw;
            }
        }

        public Task<bool> ContainsKeyAsync(string key)
        {
            var filePath = GetFilePath(key);
            return Task.FromResult(File.Exists(filePath));
        }

        public Task ClearAsync()
        {
            try
            {
                if (Directory.Exists(_storagePath))
                {
                    Directory.Delete(_storagePath, true);
                    Directory.CreateDirectory(_storagePath);
                    _logger.LogInformation("Хранилище очищено");
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке хранилища");
                throw;
            }
        }

        private string GetFilePath(string key)
        {
            // Заменяем недопустимые символы в имени файла
            var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_storagePath, $"{safeKey}.json");
        }
    }
}