//Создаем репозиторий для User
using ElectronicsComponentWarehouse.Domain.Entities;
using ElectronicsComponentWarehouse.Domain.Interfaces.Repositories;
using ElectronicsComponentWarehouse.Infrastructure.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с пользователями
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(
            ApplicationDbContext context,
            ILogger<UserRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by username: {Username}", username);
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by email: {Email}", email);
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if username exists: {Username}", username);
            return await _dbSet.AnyAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if email exists: {Email}", email);
            return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        public async Task UpdateLastLoginAsync(int userId, DateTime loginTime, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Updating last login for user ID {UserId} to {LoginTime}", userId, loginTime);
            
            var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            user.LastLoginAt = loginTime;
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated last login for user ID {UserId} to {LoginTime}", userId, loginTime);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(Domain.Enums.UserRole role, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting users by role: {Role}", role);
            return await _dbSet
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.Username)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // Переопределяем метод GetAllAsync для фильтрации неактивных пользователей
        public override async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all active users");
            return await _dbSet
                .Where(u => u.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // Переопределяем метод GetByIdAsync для проверки активности
        public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by ID {Id} (active only)", id);
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }
    }
}
