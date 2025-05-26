namespace WalletAPI.Models.DTOs
{
    public class WithdrawAndDepositTransactionDto
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
