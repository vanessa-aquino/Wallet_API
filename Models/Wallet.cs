namespace WalletAPI.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Transaction> Transactions { get; set; }

        public Wallet()
        {
            CreatedAt = DateTime.Now;
            Active = true;
        }

        public Wallet(User user)
        {
            User = user;
            UserId = user.Id;
            CreatedAt = DateTime.Now;
            Active = true;
            Balance = 0.0;
        }

        public bool IsActive()
        {
            return Active;
        }

        public void Activate()
        {
            Active = true;
        }

        public void Deactivate()
        {
            Active = false;
        }

    }


}
