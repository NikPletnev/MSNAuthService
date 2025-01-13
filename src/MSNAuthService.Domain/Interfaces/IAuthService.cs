using MSNAuthService.Domain.Models;

namespace MSNAuthService.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, List<string> Errors)> RegisterAsync(RegisterModel registerDto);
        Task<(bool Success, string Token, string RefreshToken, List<string> Errors)> LoginAsync(LoginModel loginDto);
    }
}
