using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs.Transaction
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }
        public TransactionStatus Status { get; set; }
        public string? Description { get; set; }
        public int WalletId { get; set; }

        public SimpleUserDto User { get; set; }
    }

    public class SimpleUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
}
