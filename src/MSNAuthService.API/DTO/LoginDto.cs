using System.ComponentModel.DataAnnotations;

namespace MSNAuthService.API.DTO
{
    public class LoginDto
    {
        /// <summary>
        /// Email пользователя. Используется для входа.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Пароль пользователя. Используется для проверки аутентификации.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
