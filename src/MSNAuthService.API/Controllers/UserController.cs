using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using MSNAuthService.API.DTO;
using MSNAuthService.Domain.Interfaces;
using System.Security.Claims;

namespace MSNAuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("update-email")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateEmailDto model)
        {
            var user = await _userRepository.GetUserByIdAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Email = model.Email; // Можно добавить валидацию

            await _userRepository.UpdateUserAsync(user);
            return Ok("Profile updated successfully.");
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            await _userRepository.UpdatePasswordAsync(userId, model.NewPassword);
            return Ok("Password changed successfully.");
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoleToUser(AssignRoleDto model)
        {
            await _userRepository.AssignRoleToUserAsync(model.UserId, model.RoleName);
            return Ok("Role assigned successfully.");
        }

    }
}
