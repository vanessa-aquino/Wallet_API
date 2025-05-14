using Microsoft.EntityFrameworkCore;
using WalletAPI.Data;
using WalletAPI.Interfaces;
using WalletAPI.Models;

namespace WalletAPI.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context)
        {
            _context = context;
        } 

        public async Task<Wallet> AddAsync(Wallet wallet)
        {
            try
            {
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
                return wallet;
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while adding the wallet to the database.", dbEx);

            }
        }

        public async Task<Wallet> GetByIdAsync(int id)
        {
            try
            {
                var wallet = await _context.Wallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (wallet == null)
                {
                    throw new KeyNotFoundException($"Wallet with ID {id} not found.");
                }
                return wallet;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new KeyNotFoundException("Wallet not found", knfEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the wallet.", ex);
            }
        }

        public async Task<Wallet> GetWalletByUserIdAsync(int userId)
        {
            try
            {
                var wallet = await _context.Wallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if(wallet == null)
                {
                    throw new KeyNotFoundException($"Wallet for user with ID {userId} not found.");
                }

                return wallet;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new KeyNotFoundException($"Wallet for user with ID {userId} not found.", knfEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the wallet by user ID.", ex);
            }
        } 
        
        public async Task<IEnumerable<Wallet>> GetAllAsync()
        {
            try
            {
                return await _context.Wallets
                    .Include(w => w.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the wallet.", ex);
            }
        }

        public async Task UpdateAsync(Wallet wallet)
        {
            try
            {
                var existingWallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.Id == wallet.Id);

                if (existingWallet == null)
                {
                    throw new KeyNotFoundException($"Wallet with id {wallet.Id} not found");

                }

                _context.Entry(wallet).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                throw new KeyNotFoundException($"Wallet with id {wallet.Id} not found", dbEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while updating the wallet.", dbEx);
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
                var wallet = await _context.Wallets.FindAsync(id);
                if(wallet == null)
                {
                    throw new KeyNotFoundException($"Wallet with ID {id} not found.");
                }
                _context.Wallets.Remove(wallet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new("Wallet not found", knfEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while deleting the wallet.", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred.", ex);
            }
        }
    }
}
