using WalletAPI.Models.Enums;
using WalletAPI.Models;

namespace WalletAPI.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction> AddAsync(Transaction transaction);
        Task<Transaction> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task UpdateAsync(Transaction transaction);
        Task<bool> DeleteAsync(int id);
        public Task<bool> IsFirstWithdrawOfMonthAsync(int userId);
        Task<int> CountByWalletIdAsync(int walletId);
        Task<IEnumerable<Transaction>> GetFilteredAsync(int walletId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            TransactionStatus? status = null,
            TransactionType? type = null
        );
    }
}
