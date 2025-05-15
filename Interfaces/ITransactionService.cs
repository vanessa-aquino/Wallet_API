using WalletAPI.Models;
using WalletAPI.Models.Enums;

namespace WalletAPI.Interfaces
{
    public interface ITransactionService
    {
        // Criacao e manipulacao de transacoes
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> DepositAsync(int walletId, double amount, string description);
        Task<Transaction> WithdrawAsync(int walletId, double amount, string description);
        Task<Transaction> TransferAsync(int sourceWalletId, int destinationWalletID, double amount, string description);
        Task RevertTransactionAsync(int transactionId);
        
        // Consultas e relatorios
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int walletId, DateTime? startDate, DateTime? endDate);
        Task<int> GetTotalTransactionsAsync(int walletId);
        Task<double> GetBalanceAsync(int walletId);
        Task<IEnumerable<Transaction>> GetTransactionByTypeAsync(int walletId, TransactionType type);
        Task<IEnumerable<Transaction>> GetTransactionByStatusAsync(int walletId, TransactionStatus status);
        Task<string> GenerateTransactionReportAsync(int walletId);

        // validacao e utilidades
        Task ValidateTransactionAsync(Transaction transaction);
        Task ValidateFundsAsync(int walletId, double amount);
        Task<double> CalculateTransactionFeesAsync(double amount);
    }
}
