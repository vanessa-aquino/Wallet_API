using BCrypt.Net;
using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string Email { get; set; }

        [RegularExpression(@"\(?\d{2}\)?\s?\d{5}-\d{4}", ErrorMessage = "Formato de telefone inválido. Use 99 99999-9999.")]
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public string Role { get; set; }
        public int? WalletId { get; set; }
        public Wallet? Wallet { get; set; }
        public List<Transaction> Transactions { get; set; }

        public User()
        {
            CreatedAt = DateTime.Now;
            Active = true;
            Role = "User";
        }

        public User(int id, string firstName, string lastName, DateOnly birthDate, string email, string phone, string passwordHash)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            Email = email;
            Phone = phone;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.Now;
            Active = true;
            Role = "User";
        }

        public void SetPassword(string password) => PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        public bool VerifyPassword(string password) => BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        public bool IsActive() => Active;
        public void Activate() => Active = true;
        public void Deactivate() => Active = false;
        public TimeSpan GetAccountAge() => DateTime.Now.Subtract(CreatedAt);
        public void UpdateProfile(string? firstName, string? lastName, string? email, string? phone)
        {
            if(!string.IsNullOrWhiteSpace(firstName) && firstName != "string")
                FirstName = firstName;
            if(!string.IsNullOrWhiteSpace(lastName) && lastName != "string")
                LastName = lastName;
            if(!string.IsNullOrWhiteSpace(email) && email != "string")
                Email = email;
            if(!string.IsNullOrWhiteSpace(phone) && phone != "string")
                Phone = phone;
        }


    }


}
