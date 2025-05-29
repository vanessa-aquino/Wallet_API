using System.Security.Claims;
using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> CreateWalletAsync(User user);
        Task<WalletDto> ActivateWalletAsync(int walletId);
        Task<WalletDto> DeactivateWalletAsync(int walletId);
        Task<decimal> GetBalanceAsync(int walletId, int currentUserId);
        Task ValidateSufficientFunds(int walletId, int currentUserId ,decimal amount);
        Task<WalletDto?> GetWalletByUserIdAsync(int userId);
        Task<WalletDto> GetWalletByIdAsync(int walletId);
        Task<bool> HasAccessAsync(int walletId, int userId, ClaimsPrincipal userClaims);
        bool HasAccessToUser(int targetUserId, int loggedUserId, ClaimsPrincipal userClaims);
        Task<IEnumerable<AllWalletsDto>> GetAllWalletsAsync();
        Task DeleteWalletAsync(int walletId);
    }
}
