using Microsoft.AspNetCore.Mvc;
using WalletAPI.Models;
using WalletAPI.Models.DTOs;
using WalletAPI.Models.DTOs.Transaction;
using WalletAPI.Models.Enums;

namespace WalletAPI.Interfaces.Services
{
    public interface ITransactionService
    {
        // Criacao e manipulacao de transacoes
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> DepositAsync(WithdrawAndDepositTransactionDto dto, int loggedUserId);
        Task<Transaction> WithdrawAsync(WithdrawAndDepositTransactionDto dto, int loggedUserId);
        Task<Transaction> TransferAsync(TransferTransactionDto dto, int loggedUserId);
        Task RevertTransactionAsync(int transactionId);
        
        // Consultas e relatorios
        Task<IEnumerable<TransactionResponseDto>> GetTransactionHistoryAsync(TransactionFilterDto filterDto);
        Task<int> GetTotalTransactionsAsync(int walletId);
        Task<TransactionDto> GetByIdAsync(int id);
        Task<IEnumerable<TransactionResponseDto>> GetAllAsync();

        // validacao e utilidades
        Task<FileContentResult> GenerateTransactionReportAsync(int walletId, DateTime? startDate = null, DateTime? endDate = null);
        Task ValidateFundsAsync(int walletId, decimal amount);
    }
}
