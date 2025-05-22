using Microsoft.AspNetCore.Mvc;
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
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int walletId, DateTime? startDate, DateTime? endDate);
        Task<int> GetTotalTransactionsAsync(int walletId);
        Task<double> GetBalanceAsync(int walletId);
        Task<IEnumerable<Transaction>> GetTransactionByTypeAsync(int walletId, TransactionType type);
        Task<IEnumerable<Transaction>> GetTransactionByStatusAsync(int walletId, TransactionStatus status);
        Task<FileContentResult> GenerateTransactionReportAsync(int walletId, DateTime? startDate, DateTime? endDate);

        // validacao e utilidades
        Task ValidateTransactionAsync(Transaction transaction);
        Task ValidateFundsAsync(int walletId, double amount);
        double CalculateTransactionFeesAsync(double amount, TransactionType transactionType);
    }
}
