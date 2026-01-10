//Сервис пользователей
using AutoMapper;
using ElectronicsComponentWarehouse.Application.DTOs.Users;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using ElectronicsComponentWarehouse.Domain.Common;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Application.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с пользователями
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all users");
            
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", id);
            
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return null;
            }
            
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user by username: {Username}", username);
            
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User with username {Username} not found", username);
                return null;
            }
            
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new user: {Username}", createDto.Username);
            
            // Проверяем уникальность имени пользователя
            var usernameExists = await _userRepository.UsernameExistsAsync(createDto.Username, cancellationToken);
            if (usernameExists)
            {
                _logger.LogError("User creation failed: Username {Username} already exists", createDto.Username);
                throw new ArgumentException($"Username '{createDto.Username}' already exists");
            }
            
            // Проверяем уникальность email
            var emailExists = await _userRepository.EmailExistsAsync(createDto.Email, cancellationToken);
            if (emailExists)
            {
                _logger.LogError("User creation failed: Email {Email} already exists", createDto.Email);
                throw new ArgumentException($"Email '{createDto.Email}' already exists");
            }
            
            // Парсим роль
            if (!Enum.TryParse<Domain.Enums.UserRole>(createDto.Role, true, out var userRole))
            {
                userRole = Domain.Enums.UserRole.User;
            }
            
            // Маппим DTO в сущность
            var user = _mapper.Map<Domain.Entities.User>(createDto);
            user.Role = userRole;
            
            // Устанавливаем временные метки
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            
            // Хэшируем пароль (в реальном приложении это делается в AuthService)
            // Здесь для простоты оставляем как есть, но в продакшене нужно вызывать AuthService
            
            // Сохраняем пользователя
            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            
            _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
            
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            
            // Получаем существующего пользователя
            var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found", id);
                throw new EntityNotFoundException($"User with ID {id} not found");
            }
            
            // Проверяем уникальность email (если изменился)
            if (existingUser.Email != updateDto.Email)
            {
                var emailExists = await _userRepository.EmailExistsAsync(updateDto.Email, cancellationToken);
                if (emailExists)
                {
                    _logger.LogError("User update failed: Email {Email} already exists", updateDto.Email);
                    throw new ArgumentException($"Email '{updateDto.Email}' already exists");
                }
            }
            
            // Парсим роль
            if (!Enum.TryParse<Domain.Enums.UserRole>(updateDto.Role, true, out var userRole))
            {
                _logger.LogError("Invalid role: {Role}", updateDto.Role);
                throw new ArgumentException($"Invalid role: {updateDto.Role}");
            }
            
            // Маппим обновления в существующую сущность
            existingUser.Email = updateDto.Email;
            existingUser.FullName = updateDto.FullName;
            existingUser.Role = userRole;
            
            // Сохраняем изменения
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
            
            _logger.LogInformation("User with ID {UserId} updated successfully", id);
            
            return _mapper.Map<UserDto>(existingUser);
        }

        public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            
            // Нельзя удалить самого себя (проверка в контроллере)
            // Нельзя удалить последнего администратора (проверка в контроллере)
            
            var result = await _userRepository.DeleteByIdAsync(id, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("User with ID {UserId} deleted successfully", id);
            }
            else
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            }
            
            return result;
        }

        public async Task<UserDto> UpdateUserRoleAsync(int id, string role, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating role for user ID: {UserId} to {Role}", id, role);
            
            // Получаем существующего пользователя
            var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found", id);
                throw new EntityNotFoundException($"User with ID {id} not found");
            }
            
            // Парсим роль
            if (!Enum.TryParse<Domain.Enums.UserRole>(role, true, out var userRole))
            {
                _logger.LogError("Invalid role: {Role}", role);
                throw new ArgumentException($"Invalid role: {role}");
            }
            
            existingUser.Role = userRole;
            
            // Сохраняем изменения
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
            
            _logger.LogInformation("Role for user ID {UserId} updated to {Role}", id, role);
            
            return _mapper.Map<UserDto>(existingUser);
        }

        public async Task<UserDto> SetUserActiveStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Setting active status for user ID: {UserId} to {IsActive}", id, isActive);
            
            // Получаем существующего пользователя
            var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found", id);
                throw new EntityNotFoundException($"User with ID {id} not found");
            }
            
            existingUser.IsActive = isActive;
            
            // Сохраняем изменения
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
            
            _logger.LogInformation("Active status for user ID {UserId} set to {IsActive}", id, isActive);
            
            return _mapper.Map<UserDto>(existingUser);
        }
    }
}
