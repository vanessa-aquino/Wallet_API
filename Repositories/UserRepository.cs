using Microsoft.EntityFrameworkCore;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Data;
using WalletAPI.Exceptions;

namespace WalletAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
         
        public async Task<User> AddAsync(User user)
        {
            Console.WriteLine("Entrou no AddAsync");
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while adding the user to the database.", dbEx);
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users
                     .Include(u => u.Wallet)
                     .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    throw new UserNotFoundException(id);
                }

                return user;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new KeyNotFoundException("User not found", knfEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the user.", ex);
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the user by email.", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users.Include(u => u.Wallet).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while fetching the user.", ex);
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (existingUser == null)
                {
                    throw new UserNotFoundException(user.Id);
                }

                if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.Email))
                {
                    throw new ArgumentException("User data is incomplete or invalid");
                }

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                throw new KeyNotFoundException($"User with id {user.Id} not found", dbEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while updating the user.", dbEx);
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
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    throw new UserNotFoundException(id);
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (KeyNotFoundException knfEx)
            {
                throw new KeyNotFoundException("User not found", knfEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("An error occurred while deleting the user.", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred.", ex);
            }
        }
    }
}
