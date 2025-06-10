using WalletAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using WalletAPI.Models.DTOs.User;
using WalletAPI.Exceptions;
using WalletAPI.Models;
using WalletAPI.Data;
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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var user = await _context.Users
                 .Include(u => u.Wallet)
                 .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new UserNotFoundException(id);

            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Wallet)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(User user)
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

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                throw new UserNotFoundException(id);

            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;

            if (user.Wallet != null)
            {
                user.Wallet.IsDeleted = true;
                user.Wallet.DeletedAt = DateTime.Now;
                user.Wallet.Active = false;
            }

            _context.Users.Update(user);

            await _context.SaveChangesAsync();
            return true;
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
