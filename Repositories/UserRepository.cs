using Microsoft.EntityFrameworkCore;
using WalletAPI.Models;
using WalletAPI.Data;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces.Repositories;
using WalletAPI.Models.DTOs.User;
using WalletAPI.Models.DTOs;

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
                    throw new UserNotFoundException(user.Id);

                if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.Email))
                    throw new ArgumentException("User data is incomplete or invalid");

                _context.Users.Update(user);
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
                var user = await _context.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                    throw new UserNotFoundException(id);

                user.IsDeleted = true;
                user.DeletedAt = DateTime.Now;

                if(user.Wallet != null)
                {
                    user.Wallet.IsDeleted = true;
                    user.Wallet.DeletedAt = DateTime.Now;
                    user.Wallet.Active = false;
                }

                _context.Users.Update(user);

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

        public async Task<PagedResultDto<UserProfileDto>> PaginationAsync(UserQueryParams dto)
        {
            if (dto.Page < 1) dto.Page = 1;
            if (dto.PageSize <= 0) dto.PageSize = 10;

            IQueryable<User> query;

            if (dto.OnlyDeleted == true)
                query = _context.Users.IgnoreQueryFilters().Where(u => u.IsDeleted);
            else
                query = _context.Users.AsQueryable();

            if (dto.Active.HasValue)
                query = query.Where(u => u.Active == dto.Active.Value);

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                var search = dto.Search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search));
            }

            var totalItems = await query.CountAsync();


            var users = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var userDto = users.Select(u => new UserProfileDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                BirthDate = u.BirthDate,
                Email = u.Email,
                Phone = u.Phone,
                Active = u.Active,
                CreatedAt = u.CreatedAt
            }).ToList();

            return new PagedResultDto<UserProfileDto>
            {
                Users = userDto,
                TotalUsers = totalItems,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)dto.PageSize)

            };
        }
    }
}
