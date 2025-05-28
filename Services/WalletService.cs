using Microsoft.Extensions.Caching.Memory;
using WalletAPI.Models.DTOs;
using WalletAPI.Interfaces;
using WalletAPI.Exceptions;
using WalletAPI.Models;
using System.Security.Claims;

namespace WalletAPI.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WalletService> _logger;
        private const string WalletCacheKey = "Wallet_User_";
        private const string BalanceCacheKey = "Wallet_Balance_";

        public WalletService(IWalletRepository walletRepository, IUserService userService, IMemoryCache cache, ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _userService = userService;
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
        
        public async Task<bool> HasAccessAsync(int walletId, int userId, ClaimsPrincipal userClaims)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
                throw new WalletNotFoundException(walletId);

            var isOwner = wallet.UserId == userId;
            var isAdmin = userClaims.IsInRole("Admin");

            return isOwner || isAdmin;
        }

        public async Task<WalletDto> ActivateWalletAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId)
                        ?? throw new WalletNotFoundException(walletId);
            
            wallet.Activate();
            await _walletRepository.UpdateAsync(wallet);

            InvalidateCache(wallet.UserId, walletId);
            _logger.LogInformation($"Wallet with ID {walletId} activated.");

            return new WalletDto
            {
                Id = walletId,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                Active = wallet.Active,
                CreatedAt = wallet.CreatedAt,
                UserName = $"{wallet.User?.FirstName} {wallet.User?.LastName}"
            };
        }

        public async Task<WalletDto> DeactivateWalletAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId)
                         ?? throw new WalletNotFoundException(walletId);

            if (wallet.Balance > 0)
                throw new InvalidOperationException("The wallet cannot be deactivate while there is a balance.");

            wallet.Deactivate();
            await _walletRepository.UpdateAsync(wallet);

            InvalidateCache(wallet.UserId, walletId);
            _logger.LogInformation($"Wallet with ID {walletId} deactivated.");

            return new WalletDto
            {
                Id = walletId,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                Active = wallet.Active,
                CreatedAt = wallet.CreatedAt,
                UserName = $"{wallet.User?.FirstName} {wallet.User?.LastName}"
            };
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

            user.WalletId = wallet.Id;
            await _userService.UpdateAsync(user);

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

        public async Task<decimal> GetBalanceAsync(int walletId, int currentUserId)
        {
            var cacheKey = $"{WalletCacheKey}Balance_{walletId}";

            if (!_cache.TryGetValue(cacheKey, out decimal balance))
            {
                _logger.LogInformation($"Fetching balance for wallet ID {walletId} from the database.");
                var wallet = await _walletRepository.GetByIdAsync(walletId);

                if( wallet == null )
                {
                    _logger.LogWarning($"Wallet with ID {walletId} not found.");
                    throw new WalletNotFoundException(walletId);
                }

                if(wallet.UserId != currentUserId)
                {
                    _logger.LogWarning($"User {currentUserId} attempted to access wallet {walletId} wich does not belong to them.");
                    throw new UnauthorizedAccessException("You do not have permission to view the balance of this wallet.");
                }

                balance = wallet.Balance;

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, balance, cacheOptions);
                _logger.LogInformation($"Cached balance for wallet ID {walletId}");
            }

            return balance;
        }

        public async Task<WalletDto?> GetWalletByUserIdAsync(int userId)
        {
            var cacheKey = $"{WalletCacheKey}{userId}";

            if (!_cache.TryGetValue(cacheKey, out Wallet? wallet))
            {
                _logger.LogInformation($"Fetching wallet for user ID {userId} from the database.");
                wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, wallet, cacheOptions);
                _logger.LogInformation($"Cached balance for user ID {userId}");

            }

            if(wallet == null || wallet.User == null) return null;

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

        public async Task ValidateSufficientFunds(int walletId, int currentUserId ,decimal amount)
        {
            var balance = await GetBalanceAsync(walletId, currentUserId);
            if(balance < amount)
            {
                _logger.LogWarning($"Insufficient funds in wallet ID {walletId}. Available balance: {balance}, Required: {amount}");
                throw new WalletException($"Insufficient funds in wallet ID {walletId}. Available balance: {balance}, Required: {amount}");
            }
        }
    
        public async Task<WalletDto> GetWalletByIdAsync(int walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
                throw new WalletNotFoundException(walletId);

            return new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Active = wallet.Active,
                Balance= wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                UserName = wallet.User.FirstName + " " + wallet.User.LastName
            };
        }
    }
}
