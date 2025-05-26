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
        public int? WalletId { get; set; }
        public Wallet? Wallet { get; set; }
        public List<Transaction> Transactions { get; set; }

        public User()
        {
            CreatedAt = DateTime.Now;
            Active = true;
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
        }

        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
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

        public void UpdateProfile(string firstName, string lastName, string email, string phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
        }

        public TimeSpan GetAccountAge()
        {
            return DateTime.Now.Subtract(CreatedAt);
        }

    }


}
