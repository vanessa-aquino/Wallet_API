using System.ComponentModel.DataAnnotations;
using WalletAPI.Models.Enums;

namespace WalletAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        public DateTime Date { get; set; }
        public TransactionStatus Status { get; set; }
        [MaxLength(100)] public string? Description { get; set; }

        [Required]
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; } = default!;

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int? DestinationWalletId { get; set; }
        public Wallet? DestinationWallet { get; set; }

        public Transaction() { }

        public Transaction(decimal amount,
            TransactionType transactionType,
            TransactionStatus transactionStatus,
            string? description,
            int userId,
            int walletId,
            int? destinationWalletId)
        {
            Amount = amount;
            TransactionType = transactionType;
            Date = DateTime.Now;
            Status = transactionStatus;
            Description = description;
            UserId = userId;
            WalletId = walletId;
            DestinationWalletId = destinationWalletId;
        }
    }
}
