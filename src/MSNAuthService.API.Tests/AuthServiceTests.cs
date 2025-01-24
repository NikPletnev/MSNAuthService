using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using MSNAuthService.Domain.Options;
using MSNAuthService.Domain.Services;
using Xunit;

namespace MSNAuthService.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<ITokenRepository> _mockTokenRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockTokenRepository = new Mock<ITokenRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _jwtOptions = Options.Create(new JwtOptions
            {
                Secret = Encoding.UTF8.GetBytes("A-Very-Strong-Secret"),
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            });

            _authService = new AuthService(
                Mock.Of<IConfiguration>(),
                _mockTokenRepository.Object,
                _mockUserRepository.Object,
                _jwtOptions,
                _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnError_WhenUserAlreadyExists()
        {
            // Arrange
            var email = "test@example.com";
            var registerModel = new RegisterModel
            {
                Email = email,
                Password = "Password123"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(email))
                .ReturnsAsync(new User { Email = email });

            // Act
            var result = await _authService.RegisterAsync(registerModel);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("User with this email already exists.", result.Errors);
            _mockLogger.Verify(
                log => log.LogInformation(It.IsAny<string>(), email), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUser_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "test@example.com";
            var registerModel = new RegisterModel
            {
                Email = email,
                Password = "Password123"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(email))
                .ReturnsAsync((User)null);

            _mockUserRepository
                .Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _mockUserRepository
                .Setup(repo => repo.AssignRoleToUserAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(registerModel);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnError_WhenUserDoesNotExist()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "nonexistent@example.com",
                Password = "InvalidPassword"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(loginModel.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.LoginAsync(loginModel);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid email or password.", result.Errors);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword"),
                Roles = new List<Role> { new Role { Name = "User" } }
            };

            var loginModel = new LoginModel
            {
                Email = user.Email,
                Password = "ValidPassword"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(loginModel.Email))
                .ReturnsAsync(user);

            _mockTokenRepository
                .Setup(repo => repo.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(loginModel);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnError_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var refreshToken = "InvalidToken";

            _mockTokenRepository
                .Setup(repo => repo.GetRefreshTokenAsync(refreshToken))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _authService.RefreshTokenAsync("SomeToken", refreshToken);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid or expired refresh token.", result.Errors);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = "ValidToken",
                Expires = DateTime.UtcNow.AddMinutes(10),
                IsRevoked = false
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                Roles = new List<Role> { new Role { Name = "User" } }
            };

            _mockTokenRepository
                .Setup(repo => repo.GetRefreshTokenAsync(refreshToken.Token))
                .ReturnsAsync(refreshToken);

            _mockUserRepository
                .Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            _mockTokenRepository
                .Setup(repo => repo.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RefreshTokenAsync("SomeToken", refreshToken.Token);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }
    }
}
