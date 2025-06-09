using WalletAPI.Models;
using WalletAPI.Models.DTOs;
using WalletAPI.Models.DTOs.User;

namespace WalletAPI.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);
        Task<User> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<PagedResultDto<UserProfileDto>> PaginationAsync(UserQueryParams dto);
    }
}
