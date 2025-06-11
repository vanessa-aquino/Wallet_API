namespace WalletAPI.Models.DTOs.Transaction
{
    public class WithdrawAndDepositTransactionDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public int WalletId { get; set; }
        public string? Description { get; set; }
    }
}
