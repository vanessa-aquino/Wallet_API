namespace WalletAPI.Models.DTOs
{
    public class AllWalletsDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
