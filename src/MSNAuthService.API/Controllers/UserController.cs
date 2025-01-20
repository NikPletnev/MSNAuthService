using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile()
        {
            var a = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ////var user = await _userRepository.GetUserByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //if (user == null)
            //{
            //    return NotFound("User not found.");
            //}

            //user.Email = model.Email; // Можно добавить валидацию

            //await _userRepository.UpdateUserAsync(user);
            return Ok("Profile updated successfully.");
        }

    }
}
