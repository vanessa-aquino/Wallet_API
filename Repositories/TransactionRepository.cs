using Microsoft.EntityFrameworkCore;
using WalletAPI.Models;
using WalletAPI.Data;
using WalletAPI.Models.Enums;
using WalletAPI.Interfaces.Repositories;

namespace WalletAPI.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException($"An error occurred while adding the transaction to the database (ID: {transaction.Id}).", dbEx);

            }
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Wallet)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {id} not found.");
                }
                return transaction;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new KeyNotFoundException("transaction not found", knfEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the transaction.", ex);
            }
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Wallet)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the transaction.", ex);
            }
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            try
            {
                var existingTransaction = await _context.Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == transaction.Id);

                if (existingTransaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with id {transaction.Id} not found");

                }

                _context.Entry(transaction).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                throw new KeyNotFoundException($"Transaction with id {transaction.Id} not found", dbEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while updating the transaction.", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {id} not found.");
                }
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new("Transaction not found", knfEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while deleting the transaction.", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred.", ex);
            }
        }

        public async Task<bool> IsFirstWithdrawOfMonthAsync(int userId)
        {
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var thereWasdraw = await _context.Transactions
                .AnyAsync(t => t.Status == TransactionStatus.Completed &&
                            t.UserId == userId &&
                            t.TransactionType == TransactionType.Withdraw &&
                            t.Date >= firstDayOfMonth);

            return !thereWasdraw;
        }

        public async Task<int> CountByWalletIdAsync(int walletId)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => t.WalletId == walletId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while counting transactions.", ex);
            }
        }

        public async Task<IEnumerable<Transaction>> GetFilteredAsync(
            int walletId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            TransactionStatus? status = null,
            TransactionType? type = null
        )
        {
            try
            {
                var query = _context.Transactions
                    .Include(t => t.Wallet)
                    .Where(t => t.WalletId == walletId);

                if (startDate.HasValue)
                    query = query.Where(t => t.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.Date <= endDate.Value);

                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                if (type.HasValue)
                    query = query.Where(t => t.TransactionType == type.Value);

                return await query.OrderByDescending(t => t.Date).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching filtered transactions.", ex);
            }
        }
    }

}
