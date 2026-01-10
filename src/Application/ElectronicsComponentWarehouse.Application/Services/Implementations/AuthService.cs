//Сервис аутентификации (нужно сначала создать инфраструктуру Identity)
using AutoMapper;
using ElectronicsComponentWarehouse.Application.DTOs.Auth;
using ElectronicsComponentWarehouse.Application.DTOs.Users;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using ElectronicsComponentWarehouse.Domain.Entities;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsComponentWarehouse.Application.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для аутентификации и авторизации
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting login for user: {Username}", loginDto.Username);
            
            // Находим пользователя по имени
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", loginDto.Username);
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            
            // Проверяем активность учетной записи
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Username} is inactive", loginDto.Username);
                throw new UnauthorizedAccessException("Account is inactive");
            }
            
            // Проверяем пароль
            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", loginDto.Username);
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            
            // Обновляем время последнего входа
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            // Генерируем JWT токен
            var token = GenerateJwtToken(user);
            
            _logger.LogInformation("Login successful for user: {Username}", loginDto.Username);
            
            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(GetJwtTokenExpirationHours()),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.ToString()
                }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(CreateUserDto registerDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting registration for user: {Username}", registerDto.Username);
            
            // Проверяем уникальность имени пользователя
            var usernameExists = await _userRepository.UsernameExistsAsync(registerDto.Username, cancellationToken);
            if (usernameExists)
            {
                _logger.LogError("Registration failed: Username {Username} already exists", registerDto.Username);
                throw new ArgumentException($"Username '{registerDto.Username}' already exists");
            }
            
            // Проверяем уникальность email
            var emailExists = await _userRepository.EmailExistsAsync(registerDto.Email, cancellationToken);
            if (emailExists)
            {
                _logger.LogError("Registration failed: Email {Email} already exists", registerDto.Email);
                throw new ArgumentException($"Email '{registerDto.Email}' already exists");
            }
            
            // Создаем хэш пароля
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            // Парсим роль
            if (!Enum.TryParse<Domain.Enums.UserRole>(registerDto.Role, true, out var userRole))
            {
                userRole = Domain.Enums.UserRole.User;
            }
            
            // Создаем пользователя
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = userRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            // Сохраняем пользователя
            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            
            // Генерируем JWT токен
            var token = GenerateJwtToken(createdUser);
            
            _logger.LogInformation("Registration successful for user: {Username}", registerDto.Username);
            
            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(GetJwtTokenExpirationHours()),
                User = new UserInfoDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FullName = createdUser.FullName,
                    Role = createdUser.Role.ToString()
                }
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Changing password for user ID: {UserId}", userId);
            
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogError("Change password failed: User with ID {UserId} not found", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }
            
            // Проверяем текущий пароль
            if (!VerifyPasswordHash(changePasswordDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogError("Change password failed: Current password is incorrect for user ID {UserId}", userId);
                throw new UnauthorizedAccessException("Current password is incorrect");
            }
            
            // Создаем новый хэш пароля
            CreatePasswordHash(changePasswordDto.NewPassword, out byte[] newPasswordHash, out byte[] newPasswordSalt);
            
            // Обновляем пароль
            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = newPasswordSalt;
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
            
            return true;
        }

        public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(GetJwtSecret());
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Token validation failed: {ErrorMessage}", ex.Message);
                return Task.FromResult(false);
            }
        }

        public async Task<UserInfoDto?> GetUserFromTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return null;
                }
                
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return null;
                }
                
                return new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get user from token: {ErrorMessage}", ex.Message);
                return null;
            }
        }

        public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            // В будущем можно реализовать refresh tokens
            throw new NotImplementedException("Refresh token functionality not implemented yet");
        }

        /// <summary>
        /// Генерирует JWT токен для пользователя
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecret());
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(GetJwtTokenExpirationHours()),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Создает хэш пароля с солью
        /// </summary>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Проверяет пароль по хэшу и соли
        /// </summary>
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        /// <summary>
        /// Получает секретный ключ JWT из конфигурации
        /// </summary>
        private string GetJwtSecret()
        {
            var secret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT secret is not configured");
            }
            return secret;
        }

        /// <summary>
        /// Получает время жизни JWT токена из конфигурации
        /// </summary>
        private int GetJwtTokenExpirationHours()
        {
            if (int.TryParse(_configuration["JwtSettings:TokenExpirationHours"], out int hours))
            {
                return hours;
            }
            return 24; // Значение по умолчанию: 24 часа
        }
    }
}
