using Microsoft.Extensions.Caching.Memory;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WalletService> _logger;
        private const string WalletCacheKey = "Wallet_";

        public WalletService(IWalletRepository walletRepository, IMemoryCache cache, ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task ActivateWalletAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            wallet.Activate();
            await _walletRepository.UpdateAsync(wallet);
        }

        public async Task DeactivateWalletAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            wallet.Deactivate();
            await _walletRepository.UpdateAsync(wallet);
        }

        public async Task<WalletDto> CreateWalletAsync(User user)
        {
            var wallet = new Wallet(user);
            await _walletRepository.AddAsync(wallet);

            return new WalletDto
            {
                Id = wallet.Id,
                CreatedAt = wallet.CreatedAt,
                UserId = user.Id,
                Balance = wallet.Balance,
                Active = wallet.Active,
                UserName = user.FirstName + " " + user.LastName
            };
        }

        public async Task<double> GetBalanceAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);

            if (wallet == null)
            {
                throw new KeyNotFoundException($"Wallet with ID {walletId} not found.");
            }

            return wallet.Balance;
        }

        public async Task<WalletDto> GetWalletByUserIdAsync(int userId)
        {
            var cacheKey = $"{WalletCacheKey}{userId}";

            if (!_cache.TryGetValue(cacheKey, out Wallet wallet))
            {
                _logger.LogInformation($"Fetching wallet for user ID {userId} from the database.");
                wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                
                _cache.Set(cacheKey, wallet, cacheOptions);
            }

            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new KeyNotFoundException($"Wallet for user with ID {userId} not found.");
            }

            return new WalletDto
            {
                Id = wallet.Id,
                UserId = userId,
                Active = wallet.Active,
                Balance = wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                UserName = wallet.User.FirstName + " " + wallet.User.LastName
            };


        }
    }
}
