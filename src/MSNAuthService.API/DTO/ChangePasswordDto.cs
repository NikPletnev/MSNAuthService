namespace MSNAuthService.API.DTO
{
    public class ChangePasswordDto
    {
        /// <summary>
        /// Старый пароль
        /// </summary>
        public string OldPassword { get; set; }
        /// <summary>
        /// Новый пароль
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Подтверждение нового пароля
        /// </summary>
        public string NewPasswordConfirmation { get; set; }
    }
}
