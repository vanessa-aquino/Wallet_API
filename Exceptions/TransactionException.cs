using System.Transactions;

namespace WalletAPI.Exceptions
{
    public class TransactionException : Exception
    {
        public TransactionException(string message) : base(message) { }
        public TransactionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidTransactionException : TransactionException
    {
        public InvalidTransactionException()
            : base("Invalid transaction data.") { }

        public InvalidTransactionException(Exception innerException)
            : base("Invalid transaction data.", innerException) { }
    }

    public class UnauthorizedTransactionException : TransactionException
    {
        public UnauthorizedTransactionException(int userId, int walletId)
            : base($"This user: {userId} does not have permission to perform this operation. As this wallet: {walletId} belongs to another user.") { }
    }

    public class InsufficientFundsException : TransactionException
    {
        public InsufficientFundsException()
            : base($"Insufficient balance") { }
    }

    public class TransactionLimitExceededException : TransactionException
    {
        public TransactionLimitExceededException(decimal amount, decimal limit)
            : base($"Transaction amount ({amount:C}) exceeds the allowed limit of ({limit:C}).") { }
    }

    public class TransactionCannotBeReversedException : TransactionException
    {
        public TransactionCannotBeReversedException()
            : base($"This transaction cannot be reversed.") { }
    }

    public class NotFoundException : TransactionException
    {
        public NotFoundException(int transactionId) 
            : base($"Transaction with ID: {transactionId} not found.") { }
    }
}

