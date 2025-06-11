using WalletAPI.Interfaces.Repositories;
using WalletAPI.Models.Enums;
using WalletAPI.Exceptions;
using WalletAPI.Models;

namespace WalletAPI.Validators
{
    public class TransactionValidator
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepository _userRepository;
        private const decimal TransactionLimit = 10000.00m;


        public TransactionValidator(IWalletRepository walletRepository, IUserRepository userRepository)
        {
            _walletRepository = walletRepository;
            _userRepository = userRepository;
        }
        
        public async Task ValidateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new InvalidTransactionException();

            if (transaction.Amount <= 0)
                throw new InvalidTransactionException();

            if (transaction.Amount > TransactionLimit)
                throw new TransactionLimitExceededException(transaction.Amount, TransactionLimit);

            var validTypes = Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();
            
            if (!validTypes.Contains(transaction.TransactionType))
                throw new InvalidTransactionException();

            if (transaction.Status == TransactionStatus.Canceled ||
               transaction.Status == TransactionStatus.Failed)
                    throw new InvalidTransactionException();

            var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
            
            if (wallet == null)
                throw new InvalidTransactionException();

            var user = await _userRepository.GetByIdAsync(transaction.UserId);
            
            if (user == null || wallet.UserId != user.Id)
                throw new UnauthorizedTransactionException(transaction.UserId, transaction.WalletId);
        }
    }
}
