using WalletAPI.Models;
using WalletAPI.Models.DTOs.User;

namespace WalletAPI.Interfaces.Services
{
    public interface IUserService
    {
        string GenerateToken(User user);
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<User> GetUserById(int id);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<UserDto> RegisterAsync(User user, string password);
        Task ValidateEmailAsync(string email);
        Task ActivateUserAsync(int userId);
        Task DeactivateUserAsync(int userId);
        Task<TimeSpan> GetAccountAgeAsync(int userId);
        DateTime GetTokenExpiration();
        Task UpdateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task DeleteUserAsync(int userId);
    }
}
