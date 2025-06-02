using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [RegularExpression(@"\(?\d{2}\)?\s?\d{5}-\d{4}", ErrorMessage = "Formato de telefone inválido. Use 99 99999-9999.")]
        public string Phone { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
