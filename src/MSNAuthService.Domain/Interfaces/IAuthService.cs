using MSNAuthService.Domain.Models;

namespace MSNAuthService.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterModel model);
        Task<AuthResult> LoginAsync(LoginModel model);
        Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    }
}
