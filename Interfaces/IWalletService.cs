using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> CreateWalletAsync(User user);
        Task<WalletDto> GetWalletByIdAsync(int id);
        Task ActivateWalletAsync(int walletId);
        Task DeactivateWalletAsync(int walletId);
        Task<double> GetBalanceAsync(int walletId);
    }
}
