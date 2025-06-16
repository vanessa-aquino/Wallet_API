using WalletAPI.Interfaces.Repositories;
using WalletAPI.Models.DTOs.Transaction;
using Microsoft.EntityFrameworkCore;
using WalletAPI.Models.Enums;
using WalletAPI.Models.DTOs;
using WalletAPI.Models;
using WalletAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;

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
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            var transaction = await _context.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Wallet)
                .Include(t => t.DestinationWallet)
                .ThenInclude(dw => dw.User)
                .SingleOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Wallet)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            var existingTransaction = await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == transaction.Id);

            if (existingTransaction == null)
                throw new KeyNotFoundException($"Transaction with id {transaction.Id} not found");

            _context.Entry(transaction).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFirstWithdrawOfMonthAsync(int userId)
        {
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var hasWithDraw = await _context.Transactions
                .AnyAsync(t => t.Status == TransactionStatus.Completed &&
                            t.UserId == userId &&
                            t.TransactionType == TransactionType.Withdraw &&
                            t.Date >= firstDayOfMonth);

            return !hasWithDraw;
        }

        public async Task<int> CountByWalletIdAsync(int walletId)
        {
            return await _context.Transactions
                .Where(t => t.WalletId == walletId)
                .CountAsync();
        }

        public async Task<IEnumerable<Transaction>> GetFilteredAsync(
            int walletId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            TransactionStatus? status = null,
            TransactionType? type = null
        )
        {
            var query = _context.Transactions
                .Include(t => t.Wallet)
                .Include(t => t.User)
                .Where(t => t.WalletId == walletId);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.Date <= end);
            }
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (type.HasValue)
                query = query.Where(t => t.TransactionType == type.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<Transaction> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.DestinationWallet)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Transaction with id {id} not found.");
        }

    }
}
