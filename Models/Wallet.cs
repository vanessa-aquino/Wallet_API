namespace WalletAPI.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public Wallet()
        {
            CreatedAt = DateTime.Now;
            Active = true;
            Balance = 0.0m;
            IsDeleted = false;
        }

        public Wallet(User user)
        {
            User = user;
            UserId = user.Id;
            CreatedAt = DateTime.Now;
            Active = true;
            Balance = 0.0m;
            IsDeleted = false;
        }

        public bool IsActive() => Active;
        public void Activate() => Active = true;
        public void Deactivate() => Active = false;
    }


}
