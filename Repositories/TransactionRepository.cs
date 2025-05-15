using Microsoft.EntityFrameworkCore;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Data;

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
                if(transaction == null)
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
    }
}

