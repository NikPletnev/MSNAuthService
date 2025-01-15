using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using MSNAuthService.Domain.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MSNAuthService.Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _jwtOptions;

        const string DefaultRole = "User";

        public AuthService(IConfiguration configuration, ITokenRepository tokenRepository, IUserRepository userRepository, IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
        }

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            if (await _userRepository.GetUserByEmailAsync(model.Email) != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "User with this email already exists." }
                };
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            await _userRepository.CreateUserAsync(user);

            await _userRepository.AssignRoleToUserAsync(user.Id, DefaultRole);

            return new AuthResult { Success = true };
        }

        public async Task<AuthResult> LoginAsync(LoginModel loginModel)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginModel.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash))
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "Invalid email or password." }
                };
            }

            var accessToken = GenerateAccessToken(user); 
            var refreshToken = GenerateRefreshToken(user.Id);

            await _tokenRepository.SaveRefreshTokenAsync(refreshToken);

            return new AuthResult
            {
                Success = true,
                Token = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var storedToken = await _tokenRepository.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.Expires <= DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "Invalid or expired refresh token." }
                };
            }

            var user = await _userRepository.GetUserByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "User not found." }
                };
            }

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user.Id);

            await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);
            await _tokenRepository.SaveRefreshTokenAsync(newRefreshToken);

            return new AuthResult
            {
                Success = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        private string GenerateAccessToken(User user)
        {
            var roles = user.Roles.Select(r => r.Name).ToArray();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(new SymmetricSecurityKey(_jwtOptions.Secret), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}
