using Microsoft.Extensions.Caching.Memory;
using WalletAPI.Models.DTOs;
using WalletAPI.Interfaces;
using WalletAPI.Exceptions;
using WalletAPI.Models;

namespace WalletAPI.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WalletService> _logger;
        private const string WalletCacheKey = "Wallet_User_";
        private const string BalanceCacheKey = "Wallet_Balance_";

        public WalletService(IWalletRepository walletRepository, IMemoryCache cache, ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _cache = cache;
            _logger = logger;
        }

        private void InvalidateCache(int userId, int walletId)
        {
            var walletKey = $"{WalletCacheKey}{userId}";
            var balanceKey = $"{BalanceCacheKey}{walletId}";

            _cache.Remove(walletKey);
            _cache.Remove(balanceKey);

            _logger.LogInformation($"Cache invalited for user ID {userId} and wallet ID {walletId}");
        }
        
        public async Task ActivateWalletAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            wallet.Activate();
            await _walletRepository.UpdateAsync(wallet);

            InvalidateCache(wallet.UserId, walletId);
            _logger.LogInformation($"Wallet with ID {walletId} activated.");
        }

        public async Task DeactivateWalletAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            wallet.Deactivate();
            await _walletRepository.UpdateAsync(wallet);

            InvalidateCache(wallet.UserId, walletId);
            _logger.LogInformation($"Wallet with ID {walletId} deactivated.");
        }

        public async Task<WalletDto> CreateWalletAsync(User user)
        {
            var existingWallet = await _walletRepository.GetWalletByUserIdAsync(user.Id);
            if (existingWallet != null)
            {
                _logger.LogWarning($"User with ID {user.Id} already has an active wallet.");
                throw new MultipleWalletsNotAllowedException(user.Id);
            }

            var wallet = new Wallet(user);
            await _walletRepository.AddAsync(wallet);

            InvalidateCache(user.Id, wallet.Id);
            _logger.LogInformation($"New wallet created for user ID {user.Id}.");

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
            var cacheKey = $"{WalletCacheKey}Balance_{walletId}";

            if (!_cache.TryGetValue(cacheKey, out double balance))
            {
                _logger.LogInformation($"Fetching balance for wallet ID {walletId} from the database.");
                var wallet = await _walletRepository.GetByIdAsync(walletId);
                balance = wallet.Balance;

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, balance, cacheOptions);
                _logger.LogInformation($"Cached balance for wallet ID {walletId}");
            }

            return balance;
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
                _logger.LogInformation($"Cached balance for user ID {userId}");

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

        public async Task ValidateSufficientFunds(int walletId, double amount)
        {
            var balance = await GetBalanceAsync(walletId);
            if(balance < amount)
            {
                _logger.LogWarning($"Insufficient funds in wallet ID {walletId}. Available balance: {balance}, Required: {amount}");
                throw new WalletException($"Insufficient funds in wallet ID {walletId}. Available balance: {balance}, Required: {amount}");
            }
        }
    }
}
