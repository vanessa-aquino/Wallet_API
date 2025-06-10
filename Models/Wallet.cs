
namespace WalletAPI.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public decimal Balance { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public List<Transaction> Transactions { get; set; } = new();

        public Wallet() { }
  
        public Wallet(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            UserId = user.Id;
        }

        public void Activate() => Active = true;
        public void Deactivate() => Active = false;
    }


}
