using WalletAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using WalletAPI.Exceptions;
using WalletAPI.Models;
using WalletAPI.Data;

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
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<Wallet> GetByIdAsync(int id)
        {
            var wallet = await _context.Wallets
                .Include(w => w.User)
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wallet == null)
                throw new WalletNotFoundException(id);

            return wallet;
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(int userId)
        {
            return await _context.Wallets
                .Include(w => w.User)
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<IEnumerable<Wallet>> GetAllAsync()
        {
            return await _context.Wallets
                .Include(w => w.User)
                .Include(w => w.Transactions)
                .ToListAsync();
        }

        public async Task UpdateAsync(Wallet wallet)
        {
            var existingWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == wallet.Id && !w.IsDeleted);

            if (existingWallet == null)
                throw new KeyNotFoundException($"Wallet with id {wallet.Id} not found");

            _context.Entry(wallet).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

            if (wallet == null)
                throw new KeyNotFoundException($"Wallet with ID {id} not found.");

            wallet.IsDeleted = true;
            wallet.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

    }
}
