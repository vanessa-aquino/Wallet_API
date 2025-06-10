using System.ComponentModel.DataAnnotations;
using WalletAPI.Models.Enums;

namespace WalletAPI.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Token { get; set; }
        public UserRole Role { get; set; }
    }
}
