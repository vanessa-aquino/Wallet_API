using WalletAPI.Models;

namespace WalletAPI.Interfaces.Repositories
{
    public interface IWalletRepository
    {
        Task<Wallet> AddAsync(Wallet wallet);
        Task<Wallet> GetByIdAsync(int id);
        Task<Wallet?> GetWalletByUserIdAsync(int userId);
        Task<IEnumerable<Wallet>> GetAllAsync();
        Task UpdateAsync(Wallet wallet);
        Task DeleteAsync(int id);
    }
}
