using System.Text.Json.Serialization;
using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs
{
    public class TransactionFilterDto
    {
        public int WalletId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionStatus? Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionType? TransactionType { get; set; }
    }
}
