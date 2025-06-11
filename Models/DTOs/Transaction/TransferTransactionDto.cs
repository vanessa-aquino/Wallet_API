namespace WalletAPI.Models.DTOs.Transaction
{
    public class TransferTransactionDto
    {
        public int UserId { get; set; }
        public int SourceWalletId { get; set; }
        public int DestinationWalletId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
