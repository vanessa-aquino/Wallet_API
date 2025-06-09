using WalletAPI.Models;

namespace WalletAPI.Interfaces.Repositories
{
    public interface IPasswordResetRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task AddAsync(PasswordResetToken token);
        Task UpdateAsync(PasswordResetToken token);
    }
}
