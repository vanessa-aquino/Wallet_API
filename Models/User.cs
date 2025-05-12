using BCrypt.Net;
using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public int Phone { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public int WalletID {  get; set; }
        public Wallet Wallet { get; set; }
        public List<Transaction> Transactions { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set; } 

        public User() { }

        public User(int id, string firstName, string lastName, DateTime birthDate, string email, int phone, string passwordHash)
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

        public void UpdateProfile(string firstName, string lastName,  string email, int phone)
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
