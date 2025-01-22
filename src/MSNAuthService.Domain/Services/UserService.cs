using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Services
{
    public class UserService : IUserService
    {
        public readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task AssignRoleToUserAsync(Guid id, string role)
        {
            await _userRepository.AssignRoleToUserAsync(id, role);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task UpdatePasswordAsync(Guid id, string newPassword, string oldPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                throw new Exception("Invalid password or user not found");
            }

            await _userRepository.UpdatePasswordAsync(id, newPassword);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }
    }
}
