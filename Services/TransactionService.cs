using WalletAPI.Models.DTOs.Transaction;
using WalletAPI.Interfaces.Repositories;
using WalletAPI.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Models.Enums;
using WalletAPI.Models.DTOs;
using WalletAPI.Validators;
using WalletAPI.Exceptions;
using WalletAPI.Models;
using WalletAPI.Helpers;

namespace WalletAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly TransactionReportGenerator _transactionReportGenerator;
        private readonly TransactionValidator _transactionValidator;
        private readonly TransactionFeeCalculator _transactionFeeCalculator;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository transactionRepository,
            IWalletRepository walletRepository,
            TransactionValidator transactionValidator,
            TransactionReportGenerator transactionReportGenerator,
            TransactionFeeCalculator transactionFeeCalculator,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _transactionValidator = transactionValidator;
            _transactionReportGenerator = transactionReportGenerator;
            _transactionFeeCalculator = transactionFeeCalculator;
            _logger = logger;
        }

        private TransactionStatus DetermineInitialStatus(TransactionType type)
        {
            return type switch
            {
                TransactionType.Deposit => TransactionStatus.Completed,
                TransactionType.Withdraw => TransactionStatus.Completed,
                TransactionType.Transfer => TransactionStatus.Completed,
                TransactionType.Refund => TransactionStatus.Processing,
                _ => throw new InvalidTransactionException()
            };
        }

        private TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                Date = transaction.Date,
                Status = transaction.Status,
                Description = transaction.Description,
                WalletName = $"{transaction.User.FirstName} {transaction.User.LastName}",
                DestinationWalletName = $"{transaction.DestinationWallet?.User?.FirstName ?? ""} {transaction.DestinationWallet?.User?.LastName ?? ""}"
            };
        }

        public async Task<int> GetTotalTransactionsAsync(int walletId) => await _transactionRepository.CountByWalletIdAsync(walletId);

        public async Task<FileContentResult> GenerateTransactionReportAsync(int walletId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue) startDate = new DateTime(DateTime.Now.Year, 1, 1);
            if (!endDate.HasValue) endDate = DateTime.Now;

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null) throw new WalletNotFoundException(walletId);

            var transactions = await _transactionRepository.GetFilteredAsync(walletId, startDate, endDate);
            var csvBytes = _transactionReportGenerator.GenerateCsvReport(transactions);

            var fileName = $"Relatorio_transacoes_{walletId}_{DateTime.Now:ddMMyyyy}.csv";

            return new FileContentResult(csvBytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Initiating transaction creation.");

            try
            {
                await _transactionValidator.ValidateTransactionAsync(transaction);

                if (transaction.TransactionType == TransactionType.Withdraw ||
                   transaction.TransactionType == TransactionType.Transfer)
                    await ValidateFundsAsync(transaction.WalletId, transaction.Amount);

                transaction.Date = DateTime.Now;
                transaction.Status = DetermineInitialStatus(transaction.TransactionType);

                await _transactionRepository.AddAsync(transaction);
                await _transactionRepository.SaveChangesAsync();

                _logger.LogInformation("Transaction created successfully.");
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in creating transaction.");
                throw;
            }
        }

        public async Task<TransactionDto> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException();

            return MapToDto(transaction);
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetTransactionHistoryAsync(TransactionFilterDto filterDto)
        {
            if (filterDto.WalletId <= 0)
                throw new ArgumentException("WalletId is required.");

            var transactions = await _transactionRepository.GetFilteredAsync(
                filterDto.WalletId,
                filterDto.StartDate,
                filterDto.EndDate,
                filterDto.Status,
                filterDto.TransactionType
            );

            return transactions.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                Date = t.Date,
                Status = t.Status,
                Description = t.Description,
                WalletId = t.WalletId,
                User = new SimpleUserDto
                {
                    Id = t.User.Id,
                    FullName = $"{t.User.FirstName} {t.User.LastName}"
                }

            });
        }

        public async Task ValidateFundsAsync(int walletId, decimal amount)
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

        // TRANSAÇÕES
        public async Task<Transaction> DepositAsync(WithdrawAndDepositTransactionDto dto, int loggedUserId)
        {
            _logger.LogInformation("Starting deposit.");

            var wallet = await _walletRepository.GetByIdAsync(dto.WalletId);
            if (wallet.UserId != dto.UserId)
                throw new UnauthorizedTransactionException(dto.UserId, wallet.Id);

            var transaction = new Transaction()
            {
                UserId = loggedUserId,
                Amount = dto.Amount,
                TransactionType = TransactionType.Deposit,
                WalletId = dto.WalletId,
                Description = dto.Description,
                Date = DateTime.Now,
            };

            var createdTransaction = await CreateTransactionAsync(transaction);

            wallet.Balance += dto.Amount;
            await _walletRepository.UpdateAsync(wallet);

            _logger.LogInformation("Deposit completed");
            return createdTransaction;
        }

        public async Task<Transaction> TransferAsync(TransferTransactionDto dto, int loggedUserId)
        {
            using var transaction = await _transactionRepository.BeginTransactionAsync();

            _logger.LogInformation($"Starting transfer from wallet {dto.SourceWalletId} to {dto.DestinationWalletId}.");

            try
            {

                var sourceWallet = await _walletRepository.GetByIdAsync(dto.SourceWalletId);
                if (sourceWallet == null) throw new InvalidTransactionException();
                if (sourceWallet.UserId != loggedUserId) throw new UnauthorizedTransactionException(dto.UserId, sourceWallet.Id);

                var destinationWallet = await _walletRepository.GetByIdAsync(dto.DestinationWalletId);
                if (destinationWallet == null) throw new InvalidTransactionException();

                await ValidateFundsAsync(dto.SourceWalletId, dto.Amount);

                var newTransaction = new Transaction()
                {
                    UserId = loggedUserId,
                    Amount = dto.Amount,
                    TransactionType = TransactionType.Transfer,
                    WalletId = dto.SourceWalletId,
                    Description = dto.Description,
                    Date = DateTime.Now,
                    Status = DetermineInitialStatus(TransactionType.Transfer),
                    DestinationWalletId = dto.DestinationWalletId
                };

                var fee = _transactionFeeCalculator.CalculateTransactionFees(dto.Amount, TransactionType.Transfer);
                newTransaction.Amount += fee;

                var createdTransaction = await CreateTransactionAsync(newTransaction);

                sourceWallet.Balance -= newTransaction.Amount;
                destinationWallet.Balance += dto.Amount;

                await _walletRepository.UpdateAsync(sourceWallet);
                await _walletRepository.UpdateAsync(destinationWallet);
                await _transactionRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                var fullTransaction = await _transactionRepository.GetByIdWithIncludesAsync(createdTransaction.Id);

                _logger.LogInformation($"Transfer completed from wallet {dto.SourceWalletId} to {dto.DestinationWalletId}.");
                return fullTransaction;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> WithdrawAsync(WithdrawAndDepositTransactionDto dto, int loggedUserId)
        {
            _logger.LogInformation("Starting withdrawal.");

            var wallet = await _walletRepository.GetByIdAsync(dto.WalletId);
            if (wallet.UserId != dto.UserId)
                throw new UnauthorizedTransactionException(dto.UserId, wallet.Id);

            var transaction = new Transaction()
            {
                UserId = loggedUserId,
                Amount = dto.Amount,
                TransactionType = TransactionType.Withdraw,
                WalletId = dto.WalletId,
                Description = dto.Description,
                Date = DateTime.Now
            };

            var isFirstWithdraw = await _transactionRepository.IsFirstWithdrawOfMonthAsync(loggedUserId);
            if (!isFirstWithdraw)
            {
                var fee = _transactionFeeCalculator.CalculateTransactionFees(dto.Amount, TransactionType.Withdraw);
                transaction.Amount += fee;
                _logger.LogInformation($"Withdrawal fee applied: {fee}.");
            }


            if (wallet.Balance < transaction.Amount)
                throw new InsufficientFundsException();

            var createdTransaction = await CreateTransactionAsync(transaction);

            wallet = await _walletRepository.GetByIdAsync(dto.WalletId);
            wallet.Balance -= transaction.Amount;
            await _walletRepository.UpdateAsync(wallet);

            _logger.LogInformation("Withdrawal completed");
            return createdTransaction;
        }

        public async Task RevertTransactionAsync(int transactionId)
        {
            using var dbTransaction = await _transactionRepository.BeginTransactionAsync();
            _logger.LogInformation("Starting Reversal.");

            try
            {

                var transaction = await _transactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                    throw new NotFoundException(transactionId);

                if (transaction.Status != TransactionStatus.Completed)
                    throw new TransactionCannotBeReversedException();

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
                if (wallet == null)
                    throw new NotFoundException(transaction.WalletId);

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

                await _transactionRepository.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                _logger.LogInformation($"Reversal completed.");
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetAllAsync()
        {
            var transaction = await _transactionRepository.GetAllAsync();

            return transaction.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                Date = t.Date,
                Status = t.Status,
                Description = t.Description,
                WalletId = t.WalletId,
                User = new SimpleUserDto
                {
                    Id = t.User.Id,
                    FullName = $"{t.User.FirstName} {t.User.LastName}"
                }
            });
        }
    }
}
