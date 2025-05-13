using WalletAPI.Models;

namespace WalletAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
    }
}
