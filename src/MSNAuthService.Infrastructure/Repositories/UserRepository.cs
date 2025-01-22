using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using MSNAuthService.Infrastructure.Entities;

namespace MSNAuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AuthDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("Fetching user with email: {Email}", email);
            var result = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);
            if (result != null)
            {
                _logger.LogInformation("Successfully fetched user with email: {Email}", email);
            }
            else
            {
                _logger.LogWarning("User with email: {Email} not found.", email);
            }
            return result?.Adapt<User>();
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", userId);
            var result = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (result != null)
            {
                _logger.LogInformation("Successfully fetched user with ID: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("User with ID: {UserId} not found.", userId);
            }
            return result?.Adapt<User>();
        }

        public async Task CreateUserAsync(User user)
        {
            _logger.LogInformation("Creating user with email: {Email}", user.Email);
            var userEntity = user.Adapt<UserEntity>();
            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully created user with email: {Email}", user.Email);
        }

        public async Task AssignRoleToUserAsync(Guid userId, string roleName)
        {
            _logger.LogInformation("Assigning role '{RoleName}' to user with ID: {UserId}", roleName, userId);
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !user.Roles.Any(r => r.Name == roleName))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    user.Roles.Add(role);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully assigned role '{RoleName}' to user with ID: {UserId}", roleName, userId);
                }
                else
                {
                    _logger.LogWarning("Role '{RoleName}' not found.", roleName);
                }
            }
            else
            {
                _logger.LogWarning("User with ID: {UserId} not found or already has role '{RoleName}'.", userId, roleName);
            }
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            _logger.LogInformation("Fetching roles for user with ID: {UserId}", userId);
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found.", userId);
                return new List<string>();
            }

            var roles = user.Roles.Select(r => r.Name).ToList();
            _logger.LogInformation("Successfully fetched roles for user with ID: {UserId}", userId);
            return roles;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Fetching all users.");
            var result = await _context.Users.Include(u => u.Roles).ToListAsync();
            _logger.LogInformation("Successfully fetched {Count} users.", result.Count);
            return result.Adapt<IEnumerable<User>>();
        }

        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", user.Id);
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userEntity == null)
            {
                _logger.LogError("User with ID: {UserId} not found.", user.Id);
                throw new Exception("User not found.");
            }
            userEntity.Email = user.Email;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated user with ID: {UserId}", user.Id);
        }

        public async Task UpdatePasswordAsync(Guid userId, string newPassword)
        {
            _logger.LogInformation("Updating password for user with ID: {UserId}", userId);
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated password for user with ID: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("User with ID: {UserId} not found.", userId);
            }
        }
    }
}