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
        Task<IEnumerable<Transaction>> GetListTransactionsByStatusAsync(TransactionStatus status, int walletId);
        Task<IEnumerable<Transaction>> GetListTransactionsByTypeAsync(TransactionType type, int walletId);
        Task<IEnumerable<Transaction>> GetTransactionHistoryByDate(int walletId, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<Transaction>> GetByWalletIdAndDateRangeAsync(int walletId, DateTime minDate, DateTime maxDate);
    }
}
