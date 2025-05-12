using WalletAPI.Models.Enums;

namespace WalletAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int WalletId { get; set; }
        public TransactionStatus Status { get; set; }
        public string Description { get; set; }
        public User User { get; set; }
        public Wallet Wallet { get; set; }

        public Transaction() { }

        public Transaction(double amount,  TransactionType transactionType, TransactionStatus transactionStatus, string? description)
        {
            Amount = amount;
            TransactionType = transactionType;
            Date = DateTime.Now;
            Status = transactionStatus;
            Description = description;
        }
    }
}
