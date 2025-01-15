
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MSNAuthService.Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenRepository _tokenRepository;

        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secretKey;

        public AuthService(IConfiguration configuration, ITokenRepository tokenRepository)
        {
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            _secretKey = configuration["Jwt:Secret"];
            _tokenRepository = tokenRepository;
        }

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            // Пример простой валидации (можно заменить на проверку в БД)
            if (model.Email == "existing@email.com")
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "Пользователь с таким email уже существует." }
                };
            }

            // Имитация успешной регистрации
            return await Task.FromResult(new AuthResult { Success = true });
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            // Имитация проверки пользователя (замени на реальную проверку)
            if (model.Email != "test@example.com" || model.Password != "password")
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "Неверный email или пароль." }
                };
            }

            // Генерация токена
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = GenerateAccessToken(model.Email);
            var refreshToken = GenerateRefreshToken(model.Email);

            await _tokenRepository.SaveRefreshTokenAsync(refreshToken);

            return new AuthResult
            {
                Success = true,
                Token = token,
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

            var newAccessToken = GenerateAccessToken(storedToken.UserId);
            var newRefreshToken = GenerateRefreshToken(storedToken.UserId);

            // Обновляем Refresh Token в Redis
            await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);
            await _tokenRepository.SaveRefreshTokenAsync(newRefreshToken);

            return new AuthResult
            {
                Success = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };

        }
        private string GenerateAccessToken(string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateAccessToken(string email, string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private RefreshToken GenerateRefreshToken(string userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}
