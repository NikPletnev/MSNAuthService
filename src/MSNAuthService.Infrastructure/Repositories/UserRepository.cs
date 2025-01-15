using Microsoft.EntityFrameworkCore;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using MSNAuthService.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task CreateUserAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleToUserAsync(Guid userId, string roleName)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new Exception("User not found");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) throw new Exception("Role not found");

            user.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return new List<string>();

            return user.Roles.Select(r => r.Name).ToList();
        }
    }
}
