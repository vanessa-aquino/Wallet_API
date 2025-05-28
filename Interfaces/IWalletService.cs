using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> CreateWalletAsync(User user);
        Task<WalletDto> ActivateWalletAsync(int walletId);
        Task<WalletDto> DeactivateWalletAsync(int walletId);
        Task<decimal> GetBalanceAsync(int walletId);
        Task ValidateSufficientFunds(int walletId, decimal amount);
        Task<WalletDto?> GetWalletByUserIdAsync(int userId);
        Task<WalletDto> GetWalletByIdAsync(int walletId);
    }
}
