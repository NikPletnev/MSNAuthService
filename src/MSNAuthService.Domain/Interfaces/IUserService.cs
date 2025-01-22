using MSNAuthService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Получение всех пользователей
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// Получение пользователя по id
        /// </summary>
        Task<User?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Обновление пользователя
        /// </summary>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Обновление пароля пользователя 
        /// </summary>
        Task UpdatePasswordAsync(Guid id, string newPassword, string oldPassword);

        /// <summary>
        /// Назначить новую роль пользователю
        /// </summary>
        Task AssignRoleToUserAsync(Guid id, string role);

    }
}
