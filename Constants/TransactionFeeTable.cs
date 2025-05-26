using WalletAPI.Models.Enums;

namespace WalletAPI.Constants
{
    public static class TransactionFeeTable
    {
        public static readonly Dictionary<TransactionType, decimal> FeeRates = new()
        {
            {TransactionType.Deposit, 0.0m },
            {TransactionType.Withdraw, 0.015m },
            {TransactionType.Transfer, 0.02m },
            {TransactionType.Refund, 0.0m },
        };
    }
}
