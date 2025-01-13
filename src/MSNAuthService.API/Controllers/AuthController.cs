using Mapster;
using Microsoft.AspNetCore.Mvc;
using MSNAuthService.API.DTO;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;

namespace MSNAuthService.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Маппинг DTO в модель
            var registerModel = registerDto.Adapt<RegisterModel>();
            var result = await _authService.RegisterAsync(registerModel);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Регистрация прошла успешно.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Маппинг DTO в модель
            var loginModel = loginDto.Adapt<LoginModel>();
            var result = await _authService.LoginAsync(loginModel);

            if (!result.Success)
            {
                return Unauthorized(result.Errors);
            }

            return Ok(new
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken
            });
        }
    }
}
