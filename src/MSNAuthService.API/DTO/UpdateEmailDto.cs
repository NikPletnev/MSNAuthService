using System.ComponentModel.DataAnnotations;

namespace MSNAuthService.API.DTO
{
    public class UpdateEmailDto
    {
        /// <summary>
        /// Email пользователя.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
