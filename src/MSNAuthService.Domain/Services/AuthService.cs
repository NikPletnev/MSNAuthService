using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthService> _logger;

        const string DefaultRole = "User";

        public AuthService(IConfiguration configuration,
                           ITokenRepository tokenRepository,
                           IUserRepository userRepository,
                           IOptions<JwtOptions> jwtOptions,
                           ILogger<AuthService> logger)
        {
            _jwtOptions = jwtOptions.Value;
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            _logger.LogInformation("Starting user registration for email: {Email}", model.Email);

            if (await _userRepository.GetUserByEmailAsync(model.Email) != null)
            {
                _logger.LogWarning("User with email {Email} already exists.", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "User with this email already exists." }
                };
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            };

            await _userRepository.CreateUserAsync(user);

            await _userRepository.AssignRoleToUserAsync(user.Id, DefaultRole);

            _logger.LogInformation("User registered successfully with email: {Email}", model.Email);

            return new AuthResult { Success = true };
        }

        public async Task<AuthResult> LoginAsync(LoginModel loginModel)
        {
            _logger.LogInformation("Starting login process for email: {Email}", loginModel.Email);

            var user = await _userRepository.GetUserByEmailAsync(loginModel.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}", loginModel.Email);
                return new AuthResult
                {
                    Success = false,
                    Errors = new[] { "Invalid email or password." }
                };
            }

            var accessToken = GenerateAccessToken(user); 
            var refreshToken = GenerateRefreshToken(user.Id);

            await _tokenRepository.SaveRefreshTokenAsync(refreshToken);

            _logger.LogInformation("Login successful for email: {Email}", loginModel.Email);

            return new AuthResult
            {
                Success = true,
                Token = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
        {
            _logger.LogInformation("Starting token refresh process.");

            var storedToken = await _tokenRepository.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.Expires <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token.");
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

            _logger.LogInformation("Token refresh successful.");

            return new AuthResult
            {
                Success = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        private string GenerateAccessToken(User user)
        {
            _logger.LogInformation("Generating access token for email: {Email}", user.Email);

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

            _logger.LogInformation("Access token generated successfully for email: {Email}", user.Email);

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
