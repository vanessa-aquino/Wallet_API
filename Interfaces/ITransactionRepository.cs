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
    }
}
