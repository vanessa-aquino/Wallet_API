namespace WalletAPI.Models.DTOs.User
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
