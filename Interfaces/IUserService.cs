using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Interfaces
{
    public interface IUserService
    {
        string GenerateToken(User user);
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task<UserDto> UpdateProfileAsync(int userId, string firstName, string lastName, string email, string phone);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<UserDto> RegisterAsync(User user, string password);
        Task ValidateEmailAsync(string email);
        Task ActivateUserAsync(int userId);
        Task DeactivateUserAsync(int userId);
        Task<TimeSpan> GetAccountAgeAsync(int userId);
        DateTime GetTokenExpiration();
        Task UpdateAsync(User user);
    }
}
