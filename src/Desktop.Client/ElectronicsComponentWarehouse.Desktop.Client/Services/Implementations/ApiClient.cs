using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations
{
    /// <summary>
    /// Реализация клиента для работы с API
    /// </summary>
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly JsonSerializerOptions _jsonOptions;

        public string BaseUrl
        {
            get => _httpClient.BaseAddress?.ToString() ?? string.Empty;
            set => _httpClient.BaseAddress = new Uri(value);
        }

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Настройка HttpClient
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Настройка JSON сериализации
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            // Настройка политики повторных попыток
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Повторная попытка {RetryCount} через {Delay}ms. Исключение: {ExceptionMessage}",
                            retryCount, timeSpan.TotalMilliseconds, exception.Message);
                    });
        }

        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogDebug("GET запрос: {Endpoint}", endpoint);

                    var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrEmpty(content))
                    {
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ошибка HTTP при GET запросе к {Endpoint}", endpoint);
                    throw;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации при GET запросе к {Endpoint}", endpoint);
                    throw;
                }
            });
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogDebug("POST запрос: {Endpoint}", endpoint);

                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrEmpty(responseContent))
                    {
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ошибка HTTP при POST запросе к {Endpoint}", endpoint);
                    throw;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации при POST запросе к {Endpoint}", endpoint);
                    throw;
                }
            });
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogDebug("PUT запрос: {Endpoint}", endpoint);

                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrEmpty(responseContent))
                    {
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ошибка HTTP при PUT запросе к {Endpoint}", endpoint);
                    throw;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации при PUT запросе к {Endpoint}", endpoint);
                    throw;
                }
            });
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogDebug("PATCH запрос: {Endpoint}", endpoint);

                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
                    {
                        Content = content
                    };

                    var response = await _httpClient.SendAsync(request, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrEmpty(responseContent))
                    {
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ошибка HTTP при PATCH запросе к {Endpoint}", endpoint);
                    throw;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации при PATCH запросе к {Endpoint}", endpoint);
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogDebug("DELETE запрос: {Endpoint}", endpoint);

                    var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                    return response.IsSuccessStatusCode;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ошибка HTTP при DELETE запросе к {Endpoint}", endpoint);
                    throw;
                }
            });
        }

        public void SetAuthorizationToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ClearAuthorizationToken();
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _logger.LogDebug("Установлен токен авторизации");
        }

        public void ClearAuthorizationToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _logger.LogDebug("Токен авторизации очищен");
        }

        public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("health", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось подключиться к API");
                return false;
            }
        }
    }
}