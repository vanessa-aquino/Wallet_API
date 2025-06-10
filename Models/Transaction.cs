using System.ComponentModel.DataAnnotations;
using WalletAPI.Models.Enums;

namespace WalletAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positeve.")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int WalletId { get; set; }
        public TransactionStatus Status { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        public User User { get; set; } = default!;
        public Wallet Wallet { get; set; } = default!;

        public Transaction() { }

        public Transaction(decimal amount,  TransactionType transactionType, TransactionStatus transactionStatus, string? description, int userId, int walletId)
        {
            Amount = amount;
            TransactionType = transactionType;
            Date = DateTime.Now;
            Status = transactionStatus;
            Description = description;
            UserId = userId;
            WalletId = walletId;
        }
    }
}
