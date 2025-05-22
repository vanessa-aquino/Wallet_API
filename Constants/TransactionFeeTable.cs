using WalletAPI.Models.Enums;

namespace WalletAPI.Constants
{
    public static class TransactionFeeTable
    {
        public static readonly Dictionary<TransactionType, double> FeeRates = new()
        {
            {TransactionType.Deposit, 0.0 },
            {TransactionType.Withdraw, 0.015 },
            {TransactionType.Transfer, 0.02 },
            {TransactionType.Refund, 0.0 },
        };
    }
}
