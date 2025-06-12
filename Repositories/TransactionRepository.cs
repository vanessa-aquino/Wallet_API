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
                .Include(t => t.Wallet)
                .Include(t => t.User)
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
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (type.HasValue)
                query = query.Where(t => t.TransactionType == type.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task<PagedResultDto<TransactionDto>> GetPaginationAsync(TransactionQueryParams dto)
        {
            if (dto.Page < 1) dto.Page = 1;
            if (dto.PageSize <= 0) dto.PageSize = 10;

            var query = _context.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.WalletId == dto.WalletId)
                .AsQueryable();

            if (dto.StartDate.HasValue)
                query = query.Where(t => t.Date >= dto.StartDate.Value);

            if(dto.EndDate.HasValue)
                query = query.Where(t => t.Date <= dto.EndDate.Value);

            if(dto.Status.HasValue)
                query = query.Where(t => t.Status == dto.Status.Value);

            if (dto.Type.HasValue)
                query = query.Where(t => t.TransactionType == dto.Type.Value);

            var totalItems = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((dto.Page -1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var dtoList = transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Date = t.Date,
                Description = t.Description,
                Status = t.Status,
                TransactionType = t.TransactionType,
                WalletName = $"{t.Wallet.User.FirstName} {t.Wallet.User.LastName}"
            }).ToList();

            return new PagedResultDto<TransactionDto>
            {
                Data = dtoList,
                TotalItems = totalItems,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)dto.PageSize)
            };
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
