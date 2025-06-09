using WalletAPI.Models;
using WalletAPI.Models.DTOs.PasswordReset;

namespace WalletAPI.Interfaces.Services
{
    public interface IPasswordResetService
    {
        Task<string> GenerateResetTokenAsync(string email);
        Task<PasswordResetToken> ValidateResetTokenAsync(string token);
        Task ResetPasswordAsync(string token, string newPassword);

    }
}
