using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs.Transaction
{
    public class ResponseDepositAndWithdrawDto
    {
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount {  get; set; }
        public TransactionStatus Status { get; set; }
        public string? Description {  get; set; }
        public string WalletName { get; set; } = null!;
     }
}
