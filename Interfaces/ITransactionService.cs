using WalletAPI.Models;
using WalletAPI.Models.DTOs;
using WalletAPI.Models.Enums;

namespace WalletAPI.Interfaces
{
    public interface ITransactionService
    {
        // Criacao e manipulacao de transacoes
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> DepositAsync(WithdrawAndDepositTransactionDto dto);
        Task<Transaction> WithdrawAsync(WithdrawAndDepositTransactionDto dto);
        Task<Transaction> TransferAsync(TransferTransactionDto dto);
        Task RevertTransactionAsync(int transactionId);
        
        // Consultas e relatorios
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(TransactionFilterDto filterDto);
        Task<int> GetTotalTransactionsAsync(int walletId);
        Task<TransactionResponseDto> GetByIdAsync(int id);

        // validacao e utilidades
        Task ValidateTransactionAsync(Transaction transaction);
        Task ValidateFundsAsync(int walletId, decimal amount);
        decimal CalculateTransactionFeesAsync(decimal amount, TransactionType transactionType);
    }
}
