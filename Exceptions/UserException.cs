namespace WalletAPI.Exceptions
{
    public class UserException : Exception
    {
        public UserException(string message) :base(message) { }
        public UserException(string message, Exception innerException) :base(message, innerException) { }
    }

    public class UserNotFoundException : UserException
    {
        public UserNotFoundException(int userId)
            : base($"User with ID {userId} not found.") { }

        public UserNotFoundException(int userId, Exception innerException)
            : base($"User with ID {userId} not found.", innerException) { }
    }

    public class EmailAlreadyInUseException : UserException
    {
        public EmailAlreadyInUseException(string email)
            : base($"The email '{email}' is already in use.") { }

        public EmailAlreadyInUseException(string email, Exception innerException)
            : base($"The email '{email}' is already in use.", innerException) { }
    }

    public class InvalidUserCredentialsException : UserException
    {
        public InvalidUserCredentialsException()
            : base("Invalid email or password.") { }

        public InvalidUserCredentialsException(Exception innerException)
            :base("Invalid email or password", innerException) { }
    }
}
