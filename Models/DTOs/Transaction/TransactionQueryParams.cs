using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs.Transaction
{
    public class TransactionQueryParams
    {
        public int WalletId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TransactionStatus? Status { get; set; }
        public TransactionType? Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
