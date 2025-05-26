using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs
{
    public class TransactionFilterDto
    {
        public int WalletId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TransactionStatus? Status { get; set; }
        public TransactionType? TransactionType { get; set; }
    }
}
