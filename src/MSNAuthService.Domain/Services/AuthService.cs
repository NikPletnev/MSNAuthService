
//using System.Security.Claims;
//using System.Text;
//using MSNAuthService.Domain.Entities;
//using MSNAuthService.Domain.Interfaces;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;

//namespace MSNAuthService.Domain.Services
//{
//    public class AuthService : IAuthService
//    {
//        private readonly IUserRepository _userRepository;
//        private readonly IConfiguration _configuration;
//        private readonly IPasswordHasher<User> _passwordHasher;

//        public AuthService(IUserRepository userRepository, IConfiguration configuration, IPasswordHasher<User> passwordHasher)
//        {
//            _userRepository = userRepository;
//            _configuration = configuration;
//            _passwordHasher = passwordHasher;
//        }

//        public async Task<(bool Success, List<string> Errors)> RegisterAsync(RegisterDto registerDto)
//        {
//            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
//            if (existingUser != null)
//            {
//                return (false, new List<string> { "Пользователь с таким email уже существует." });
//            }

//            var user = new User
//            {
//                Email = registerDto.Email,
//                PasswordHash = _passwordHasher.HashPassword(null, registerDto.Password),
//                Roles = new List<Role> { new Role { Name = "User" } }
//            };

//            await _userRepository.AddAsync(user);
//            return (true, new List<string>());
//        }

//        public async Task<(bool Success, string Token, string RefreshToken, List<string> Errors)> LoginAsync(LoginDto loginDto)
//        {
//            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
//            if (user == null)
//            {
//                return (false, null, null, new List<string> { "Неверный email или пароль." });
//            }

//            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
//            if (result == PasswordVerificationResult.Failed)
//            {
//                return (false, null, null, new List<string> { "Неверный email или пароль." });
//            }

//            var token = GenerateJwtToken(user);
//            var refreshToken = Guid.NewGuid().ToString();

//            // Сохранение refresh-токена (можно реализовать хранение в базе данных)
//            // await _userRepository.SaveRefreshTokenAsync(user.Id, refreshToken);

//            return (true, token, refreshToken, new List<string>());
//        }

//        private string GenerateJwtToken(User user)
//        {
//            throw new NotImplementedException();
//            //var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);
//            //var claims = new List<Claim>
//            //{
//            //    new Claim(ClaimTypes.Email, user.Email),
//            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
//            //};

//            //foreach (var role in user.Roles)
//            //{
//            //    claims.Add(new Claim(ClaimTypes.Role, role.Name));
//            //}

//            //var tokenDescriptor = new SecurityTokenDescriptor
//            //{
//            //    Subject = new ClaimsIdentity(claims),
//            //    Expires = DateTime.UtcNow.AddHours(1),
//            //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            //};

//            //var tokenHandler = new JwtSecurityTokenHandler();
//            //var token = tokenHandler.CreateToken(tokenDescriptor);
//            //return tokenHandler.WriteToken(token);
//        }
//    }
//}
