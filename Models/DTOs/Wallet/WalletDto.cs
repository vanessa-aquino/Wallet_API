namespace WalletAPI.Models.DTOs.Wallet
{
    public class WalletDto
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
