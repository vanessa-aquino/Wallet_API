namespace WalletAPI.Models.DTOs.Transaction
{
    public class TransactionResponseDto
    {
        public int Id {  get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date {  get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
        public int WalletId { get; set; }
    }
}
