using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletAPI.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }
        public bool Used {  get; private set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        public User User {  get; set; }
        
        public PasswordResetToken()
        {
            Used = false;
            CreatedAt = DateTime.Now;
            ExpirationDate = CreatedAt.AddMinutes(30);
        }

        public void MarkUsed()
        {
            Used = true;
        }
    
    }

    


}
