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

            var registerModel = registerDto.Adapt<RegisterModel>();
            var result = await _authService.RegisterAsync(registerModel);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Регистрация успешна.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginModel = loginDto.Adapt<LoginModel>();
            var result = await _authService.LoginAsync(loginModel);

            if (!result.Success)
            {
                return Unauthorized(result.Errors);
            }

            return Ok(new { result.Token, result.RefreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto.Token, refreshTokenDto.RefreshToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { result.Token, result.RefreshToken });
        }

    }
}
