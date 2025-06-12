using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs.Transaction
{
    public class ResponseTransferDto
    {
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }

        private string? _description;
        public string Description
        {
            get => _description ?? string.Empty;
            set => _description = string.IsNullOrWhiteSpace(value) || value == "string"
                ? string.Empty
                : value;
        }

        public string SourceWalletOwnerName { get; set; } = null!;
        public string DestinationWalletOwnerName { get; set; } = null!;
    }
}
