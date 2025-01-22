using Mapster;
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

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var result = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);
            return result.Adapt<User>();
        }

        public async Task<User?> GetUserByIdAsync(Guid userId) 
        {
            var result = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return result.Adapt<User>();
        }

        public async Task CreateUserAsync(User user)
        {
            var userEntity = user.Adapt<UserEntity>();
            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleToUserAsync(Guid userId, string roleName)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !user.Roles.Any(r => r.Name == roleName))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    user.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return new List<string>();

            return user.Roles.Select(r => r.Name).ToList();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var result = await _context.Users.Include(u => u.Roles).ToListAsync();
            return result.Adapt<IEnumerable<User>>();
        }

        public async Task UpdateUserAsync(User user)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userEntity == null)
            {
                throw new Exception("User not found.");
            }

            userEntity.Email = user.Email;

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePasswordAsync(Guid userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();
            }
        }
    }
}
