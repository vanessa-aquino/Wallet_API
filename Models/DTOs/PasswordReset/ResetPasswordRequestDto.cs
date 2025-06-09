namespace WalletAPI.Models.DTOs.PasswordReset
{
    public class ResetPasswordRequestDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
