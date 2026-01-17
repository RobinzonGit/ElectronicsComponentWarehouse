using System.Text.Json;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Auth;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса аутентификации
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<AuthService> _logger;

        private AuthResponseModel? _currentAuth;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserInfoModel? CurrentUser => _currentAuth?.User;
        public string? Token => _currentAuth?.Token;
        public bool IsAuthenticated => _currentAuth?.IsValid ?? false;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        public event EventHandler<bool>? AuthenticationChanged;

        public AuthService(
            IApiClient apiClient,
            ILocalStorageService localStorage,
            ILogger<AuthService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<AuthResponseModel?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Попытка входа пользователя: {Username}", username);

                var loginData = new
                {
                    username,
                    password
                };

                var response = await _apiClient.PostAsync<AuthResponseModel>(
                    "api/Auth/login",
                    loginData,
                    cancellationToken);

                if (response != null && response.IsValid)
                {
                    await SetAuthenticationAsync(response);
                    _logger.LogInformation("Пользователь {Username} успешно вошел в систему", username);
                    return response;
                }

                _logger.LogWarning("Не удалось войти в систему для пользователя: {Username}", username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя: {Username}", username);
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                _logger.LogInformation("Выход пользователя из системы");

                _currentAuth = null;
                _apiClient.ClearAuthorizationToken();
                await ClearSavedSessionAsync();

                AuthenticationChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выходе из системы");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                _apiClient.SetAuthorizationToken(token);

                var userInfo = await _apiClient.GetAsync<UserInfoModel>(
                    "api/Auth/validate",
                    cancellationToken);

                return userInfo != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Токен не валиден");
                return false;
            }
        }

        public async Task<string?> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            // В текущей реализации API не поддерживает refresh токены
            // Можно добавить позже
            _logger.LogWarning("Refresh токенов не поддерживается в текущей версии API");
            return null;
        }

        public async Task<bool> LoadSavedSessionAsync()
        {
            try
            {
                var savedAuth = await _localStorage.LoadAsync<AuthResponseModel>("auth_session");

                if (savedAuth != null && savedAuth.IsValid)
                {
                    await SetAuthenticationAsync(savedAuth);
                    _logger.LogInformation("Сессия восстановлена для пользователя: {Username}", savedAuth.User.Username);
                    return true;
                }

                if (savedAuth != null && savedAuth.IsExpired)
                {
                    _logger.LogWarning("Сохраненная сессия истекла");
                    await ClearSavedSessionAsync();
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке сохраненной сессии");
                return false;
            }
        }

        public async Task SaveSessionAsync()
        {
            try
            {
                if (_currentAuth != null)
                {
                    await _localStorage.SaveAsync("auth_session", _currentAuth);
                    _logger.LogDebug("Сессия сохранена");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении сессии");
            }
        }

        public async Task ClearSavedSessionAsync()
        {
            try
            {
                await _localStorage.DeleteAsync("auth_session");
                _logger.LogDebug("Сохраненная сессия очищена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке сохраненной сессии");
            }
        }

        private async Task SetAuthenticationAsync(AuthResponseModel authResponse)
        {
            _currentAuth = authResponse;
            _apiClient.SetAuthorizationToken(authResponse.Token);
            await SaveSessionAsync();
            AuthenticationChanged?.Invoke(this, true);
        }
    }
}