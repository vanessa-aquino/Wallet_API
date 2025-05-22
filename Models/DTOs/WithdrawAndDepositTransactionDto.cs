namespace WalletAPI.Models.DTOs
{
    public class WithdrawAndDepositTransactionDto
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }
        public string? Description { get; set; }
    }
}
