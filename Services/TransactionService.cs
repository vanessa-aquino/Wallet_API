using Microsoft.Extensions.Caching.Memory;
using WalletAPI.Models.Enums;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces;
using WalletAPI.Models;

namespace WalletAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletService _walletService;
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TransactionService> _logger;
        private const string TransactionCacheKey = "Transaction_";
        private const double TransactionLimit = 10000.00;

        private TransactionStatus DetermineInitialStatus(TransactionType type)
        {
            return type switch
            {
                TransactionType.Deposit => TransactionStatus.Completed,
                TransactionType.Withdraw => TransactionStatus.Completed,
                TransactionType.Transfer => TransactionStatus.Pending,
                TransactionType.Pix => TransactionStatus.Pending,
                TransactionType.Refund => TransactionStatus.Processing,
                _ => throw new InvalidTransactionException()
            };
        }

        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, IUserRepository userRepository, IWalletService walletService, IMemoryCache cache, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _userRepository = userRepository;
            _walletService = walletService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<double> CalculateTransactionFeesAsync(double amount, TransactionType transactionType, double tax, int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Initiating transaction creation.");

            try
            {
                await ValidateTransactionAsync(transaction);

                if (transaction.TransactionType == TransactionType.Withdraw ||
                   transaction.TransactionType == TransactionType.Transfer ||
                   transaction.TransactionType == TransactionType.Pix)
                {
                    await ValidateFundsAsync(transaction.WalletId, transaction.Amount);
                }

                transaction.Date = DateTime.UtcNow;
                transaction.Status = DetermineInitialStatus(transaction.TransactionType);

                await _transactionRepository.AddAsync(transaction);
                _logger.LogInformation("Transaction created successfully.");
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in creating transaction.");
                throw;
            }
        }

        public Task<Transaction> DepositAsync(int walletId, double amount, string? description)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateTransactionReportAsync(int walletId)
        {
            throw new NotImplementedException();
        }

        public async Task<double> GetBalanceAsync(int walletId)
        {
            return await _walletService.GetBalanceAsync(walletId);
        }

        public async Task<int> GetTotalTransactionsAsync(int walletId)
        {
            return await _transactionRepository.CountByWalletIdAsync(walletId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionByStatusAsync(int walletId, TransactionStatus status)
        {
            return await _transactionRepository.GetListTransactionsByStatusAsync(status, walletId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionByTypeAsync(int walletId, TransactionType type)
        {
            return await _transactionRepository.GetListTransactionsByTypeAsync(type, walletId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int walletId, DateTime? startDate, DateTime? endDate)
        {
            return await _transactionRepository.GetTransactionHistoryByDate(walletId, startDate, endDate);
        }

        public Task<Transaction> PixAsync(int sourceWalletId, int destinationWalletID, double amount, string? description)
        {
            throw new NotImplementedException();
        }

        public Task RevertTransactionAsync(int transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction> TransferAsync(int sourceWalletId, int destinationWalletID, double amount, string description)
        {
            throw new NotImplementedException();
        }

        public async Task ValidateFundsAsync(int walletId, double amount)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);

            if (wallet == null)
            {
                _logger.LogWarning("Null wallet.");
                throw new InvalidTransactionException();
            }

            if (wallet.Balance < amount)
            {
                _logger.LogWarning($"Wallet with insufficient balance.");
                throw new InsufficientFundsException();
            }
        }

        public async Task ValidateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                _logger.LogWarning("Null transaction received.");
                throw new InvalidTransactionException();
            }

            if (transaction.Amount <= 0)
            {
                _logger.LogWarning($"Invalid value in transaction: {transaction.Amount}.");
                throw new InvalidTransactionException();
            }

            if (transaction.Amount > TransactionLimit)
            {
                _logger.LogWarning($"Value {transaction.Amount} exceeds the permitted limit of {TransactionLimit}.");
                throw new TransactionLimitExceededException(transaction.Amount, TransactionLimit);
            }

            var validTypes = Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();
            if (!validTypes.Contains(transaction.TransactionType))
            {
                _logger.LogWarning("Invalid transaction type.");
                throw new InvalidTransactionException();
            }

            if (transaction.Status == TransactionStatus.Canceled ||
               transaction.Status == TransactionStatus.Failed)
            {
                _logger.LogWarning("Invalid transaction status.");
                throw new InvalidTransactionException();
            }

            var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet == null)
            {
                _logger.LogWarning($"Wallet with ID {wallet.Id} not found.");
                throw new InvalidTransactionException();
            }

            var user = await _userRepository.GetByIdAsync(transaction.UserId);
            if (user == null || wallet.UserId != user.Id)
            {
                _logger.LogWarning($"User with ID {user.Id} not found.");
                throw new UnauthorizedTransactionException(transaction.UserId, transaction.WalletId);
            }
        }

        public Task<Transaction> WithdrawAsync(int walletId, double amount, string description)
        {
            throw new NotImplementedException();
        }
    }
}
