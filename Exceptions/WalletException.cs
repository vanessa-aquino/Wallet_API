namespace WalletAPI.Exceptions
{
    public class WalletException : Exception
    {
        public WalletException(string message) : base(message) { }
        public WalletException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MultipleWalletsNotAllowedException : WalletException
    {
        public MultipleWalletsNotAllowedException(int userId)
            : base($"User with ID {userId} already has an active wallet.") { }
    }

    public class WalletNotFoundException : WalletException
    {
        public WalletNotFoundException(int walletId)
            : base($"Wallet with ID {walletId} not found.") { }
    }
}
