using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> CreateWalletAsync(User user);
        Task ActivateWalletAsync(int walletId);
        Task DeactivateWalletAsync(int walletId);
        Task<double> GetBalanceAsync(int walletId);
        Task ValidateSufficientFunds(int walletId, double amount);
    }
}
