using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Models.Enums;

namespace WalletAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly Wallet _wallet;
        private const double TransactionLimit = 10000.00;

        public async Task<double> CalculateTransactionFeesAsync(double amount, TransactionType transactionType, double tax, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction> DepositAsync(int walletId, double amount, string description)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateTransactionReportAsync(int walletId)
        {
            throw new NotImplementedException();
        }

        public Task<double> GetBalanceAsync(int walletId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalTransactionsAsync(int walletId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Transaction>> GetTransactionByStatusAsync(int walletId, TransactionStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Transaction>> GetTransactionByTypeAsync(int walletId, TransactionType type)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int walletId, DateTime? startDate, DateTime? endDate)
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

        public Task ValidateFundsAsync(int walletId, double amount)
        {
            throw new NotImplementedException();
        }

        public Task ValidateTransactionAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction> WithdrawAsync(int walletId, double amount, string description)
        {
            throw new NotImplementedException();
        }
    }
}
