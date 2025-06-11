using WalletAPI.Models.Enums;
using WalletAPI.Constants;

namespace WalletAPI.Helpers
{
    public class TransactionFeeCalculator
    {
        public decimal CalculateTransactionFees(decimal amount, TransactionType transactionType)
        {
            if (!TransactionFeeTable.FeeRates.ContainsKey(transactionType))
                throw new InvalidOperationException($"Transaction type '{transactionType}' is not supported for fee calculation.");

            var taxRate = TransactionFeeTable.FeeRates[transactionType];
            return amount * taxRate;
        }

    }
}
