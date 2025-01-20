using MSNAuthService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User user);
        Task AssignRoleToUserAsync(Guid userId, string roleName);
        Task<List<string>> GetUserRolesAsync(Guid userId);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
    }
}
