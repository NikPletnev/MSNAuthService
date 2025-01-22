using Microsoft.Extensions.Logging;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task AssignRoleToUserAsync(Guid id, string role)
    {
        try
        {
            await _userRepository.AssignRoleToUserAsync(id, role);
            _logger.LogInformation("Role '{Role}' assigned to user with ID: {UserId}", role, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role '{Role}' to user with ID: {UserId}", role, id);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync();
            _logger.LogInformation("Fetched users from repository.");
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users from repository.");
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found.", id);
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task UpdatePasswordAsync(Guid id, string newPassword, string oldPassword)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password or user not found for ID: {UserId}", id);
                throw new ArgumentException("Invalid password or user not found");
            }
            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdatePasswordAsync(id, newHashedPassword);
            _logger.LogInformation("Password updated for user with ID: {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation("User with ID: {UserId} updated.", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
            throw;
        }
    }
}