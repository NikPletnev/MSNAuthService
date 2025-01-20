using System.ComponentModel.DataAnnotations;

namespace MSNAuthService.API.DTO
{
    public class RegisterDto
    {
        /// <summary>
        /// Email пользователя.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [Required]
        [MinLength(8, ErrorMessage = "Пароль должен содержать не менее 8 символов.")]
        public string Password { get; set; }

        /// <summary>
        /// Повторение пароля для проверки совпадения.
        /// </summary>
        [Required]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Имя пользователя 
        /// </summary>
        public string? FullName { get; set; }
    }
}
