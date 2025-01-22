using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using MSNAuthService.API.DTO;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using System.Security.Claims;

namespace MSNAuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;

        public UserController(IUserService userRepository)
        {
            _userService = userRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("update-email")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateEmailDto model)
        {

            await _userService.UpdateUserAsync(new User { Id = Guid.Parse(User.FindFirstValue("id")), Email = model.Email });
            return Ok("Profile updated successfully.");
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            await _userService.UpdatePasswordAsync(Guid.Parse(User.FindFirstValue("id")), model.NewPassword, model.OldPassword);
            return Ok("Password changed successfully.");
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoleToUser(AssignRoleDto model)
        {
            await _userService.AssignRoleToUserAsync(model.UserId, model.RoleName);
            return Ok("Role assigned successfully.");
        }

    }
}
