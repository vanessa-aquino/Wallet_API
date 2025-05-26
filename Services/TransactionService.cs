using Microsoft.Extensions.Caching.Memory;
using WalletAPI.Models.Enums;
using WalletAPI.Models.DTOs;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces;
using WalletAPI.Constants;
using WalletAPI.Models;
using WalletAPI.Data;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WalletAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletService _walletService;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionService> _logger;
        private const double TransactionLimit = 10000.00;

        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, IUserRepository userRepository, IWalletService walletService, IMemoryCache cache, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _userRepository = userRepository;
            _walletService = walletService;
            _logger = logger;
        }

        private TransactionStatus DetermineInitialStatus(TransactionType type)
        {
            return type switch
            {
                TransactionType.Deposit => TransactionStatus.Completed,
                TransactionType.Withdraw => TransactionStatus.Completed,
                TransactionType.Transfer => TransactionStatus.Pending,
                TransactionType.Refund => TransactionStatus.Processing,
                _ => throw new InvalidTransactionException()
            };
        }

        private string EscapeCsv(string input)
        {
            if(string.IsNullOrEmpty(input)) return "";
            if(input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            {
                input = input.Replace("\"", "\"\"");
                return $"\"{input}\"";
            }
            return input;
        }

        public double CalculateTransactionFeesAsync(double amount, TransactionType transactionType)
        {
            if (!TransactionFeeTable.FeeRates.ContainsKey(transactionType))
            {
                throw new InvalidOperationException($"Transaction type '{transactionType}' is not supported for fee calculation.");
            }

            var taxRate = TransactionFeeTable.FeeRates[transactionType];
            return amount * taxRate;
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Initiating transaction creation.");

            try
            {
                await ValidateTransactionAsync(transaction);

                if (transaction.TransactionType == TransactionType.Withdraw ||
                   transaction.TransactionType == TransactionType.Transfer)
                {
                    await ValidateFundsAsync(transaction.WalletId, transaction.Amount);
                }

                transaction.Date = DateTime.Now;
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

        public async Task<Transaction> DepositAsync(WithdrawAndDepositTransactionDto dto)
        {
            _logger.LogInformation("Starting deposit.");

            var transaction = new Transaction()
            {
                Amount = dto.Amount,
                TransactionType = TransactionType.Deposit,
                WalletId = dto.WalletId,
                UserId = dto.UserId,
                Description = dto.Description,
                Date = DateTime.Now
            };

            var createdTransaction = await CreateTransactionAsync(transaction);

            var wallet = await _walletRepository.GetByIdAsync(dto.WalletId);
            wallet.Balance += transaction.Amount;
            await _walletRepository.UpdateAsync(wallet);

            _logger.LogInformation("Deposit completed");
            return createdTransaction;
        }

        public async Task<FileContentResult> GenerateTransactionReportAsync(int walletId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue) startDate = new DateTime(DateTime.Now.Year, 1, 1);
            if (!endDate.HasValue) endDate = DateTime.Now;

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null) throw new WalletNotFoundException(walletId);

            var transactions = await _transactionRepository.GetFilteredAsync(walletId, startDate, endDate);

            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var lines = new List<string>
            {
                $"Id{separator}Data{separator}Tipo{separator}Valor{separator}Status{separator}Descrição"
            };

            foreach(var t in transactions)
            {
                var line = $"{t.Id}{separator}{t.Date:dd/MM/yyyy HH:mm}{separator}{t.TransactionType}{separator}R${t.Amount:F2}{separator}{t.Status}{separator}{EscapeCsv(t.Description)}";
                lines.Add(line);
            }

            var csv = string.Join(Environment.NewLine, lines);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            var fileName = $"Relatorio_transacoes_{walletId}_{DateTime.Now:ddMMyyyy}";

            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<double> GetBalanceAsync(int walletId)
        {
            return await _walletService.GetBalanceAsync(walletId);
        }

        public async Task<int> GetTotalTransactionsAsync(int walletId)
        {
            return await _transactionRepository.CountByWalletIdAsync(walletId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(TransactionFilterDto filterDto)
        {
            if (filterDto.WalletId <= 0)
                throw new ArgumentException("WalletId is required.");

            return await _transactionRepository.GetFilteredAsync(
                filterDto.WalletId,
                filterDto.StartDate,
                filterDto.EndDate,
                filterDto.Status,
                filterDto.TransactionType
            );
        }

        public async Task RevertTransactionAsync(int transactionId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Starting Reversal.");

            try
            {

                var transaction = await _transactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    throw new NotFoundException(transactionId);

                }
                if (transaction.Status != TransactionStatus.Completed)
                {
                    throw new TransactionCannotBeReversedException();
                }

                var reversalTransaction = new Transaction()
                {
                    Amount = transaction.Amount,
                    TransactionType = TransactionType.Refund,
                    WalletId = transaction.WalletId,
                    UserId = transaction.UserId,
                    Description = $"Reversal of transaction {transactionId}.",
                    Date = DateTime.Now,
                    Status = TransactionStatus.Completed
                };

                var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);

                switch (transaction.TransactionType)
                {
                    case TransactionType.Withdraw:
                    case TransactionType.Transfer:
                        wallet.Balance += transaction.Amount;
                        break;

                    case TransactionType.Deposit:
                    case TransactionType.Refund:
                        wallet.Balance -= transaction.Amount;
                        break;

                    default:
                        throw new InvalidTransactionException();
                }

                await _transactionRepository.AddAsync(reversalTransaction);
                transaction.Status = TransactionStatus.Canceled;
                await _transactionRepository.UpdateAsync(transaction);
                await _walletRepository.UpdateAsync(wallet);

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                _logger.LogInformation($"Reversal completed.");
            }
            catch(Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> TransferAsync(TransferTransactionDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            _logger.LogInformation($"Starting transfer from wallet {dto.SourceWalletId} to {dto.DestinationWalletId}.");

            try
            {
                var sourceWallet = await _walletRepository.GetByIdAsync(dto.SourceWalletId);
                if (sourceWallet == null) throw new InvalidTransactionException();
                if (sourceWallet.UserId != dto.UserId) throw new UnauthorizedAccessException();

                var destinationWallet = await _walletRepository.GetByIdAsync(dto.DestinationWalletId);
                if (destinationWallet == null) throw new InvalidTransactionException();

                await ValidateFundsAsync(dto.SourceWalletId, dto.Amount);

                var newTransaction = new Transaction()
                {
                    Amount = dto.Amount,
                    TransactionType = TransactionType.Transfer,
                    WalletId = dto.SourceWalletId,
                    UserId = dto.UserId,
                    Description = dto.Description,
                    Date = DateTime.Now,
                    Status = DetermineInitialStatus(TransactionType.Transfer)
                };

                var fee = CalculateTransactionFeesAsync(dto.Amount, TransactionType.Transfer);
                newTransaction.Amount += fee;

                var createdTransaction = await CreateTransactionAsync(newTransaction);

                sourceWallet.Balance -= newTransaction.Amount;
                destinationWallet.Balance += dto.Amount;

                await _walletRepository.UpdateAsync(sourceWallet);
                await _walletRepository.UpdateAsync(destinationWallet);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Transfer completed from wallet {dto.SourceWalletId} to {dto.DestinationWalletId}.");
                return createdTransaction;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

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

        public async Task<Transaction> WithdrawAsync(WithdrawAndDepositTransactionDto dto)
        {
            _logger.LogInformation("Starting withdrawal.");

            var transaction = new Transaction()
            {
                Amount = dto.Amount,
                TransactionType = TransactionType.Withdraw,
                WalletId = dto.WalletId,
                UserId = dto.UserId,
                Description = dto.Description,
                Date = DateTime.Now
            };

            var isFirstWithdraw = await _transactionRepository.IsFirstWithdrawOfMonthAsync(dto.UserId);
            if (!isFirstWithdraw)
            {
                var fee = CalculateTransactionFeesAsync(dto.Amount, TransactionType.Withdraw);
                transaction.Amount += fee;
                _logger.LogInformation($"Withdrawal fee applied: {fee}.");
            }

            var createdTransaction = await CreateTransactionAsync(transaction);

            var wallet = await _walletRepository.GetByIdAsync(dto.WalletId);
            wallet.Balance -= transaction.Amount;
            await _walletRepository.UpdateAsync(wallet);

            _logger.LogInformation("Withdrawal completed");
            return createdTransaction;
        }
    }
}
