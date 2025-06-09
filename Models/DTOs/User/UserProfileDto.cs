namespace WalletAPI.Models.DTOs.User
{
    public class UserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}
