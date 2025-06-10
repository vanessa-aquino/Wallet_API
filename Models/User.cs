using System.ComponentModel.DataAnnotations;
using WalletAPI.Models.Enums;

namespace WalletAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        [Required] public DateOnly BirthDate { get; set; }
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required] public string Phone { get; set; } = null!;

        [RegularExpression(@"\(?\d{2}\)?\s?\d{5}-\d{4}", ErrorMessage = "Formato de telefone inválido. Use 99 99999-9999.")]
        [Required, DataType(DataType.Password)] public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public int? WalletId { get; set; }
        public Wallet? Wallet { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();


        public User()
        {
            CreatedAt = DateTime.Now;
            Active = true;
            IsDeleted = false;
        }

        public User(string firstName, string lastName, DateOnly birthDate, string email, string phone, string passwordHash)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            Email = email;
            Phone = phone;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.Now;
            Active = true;
            IsDeleted = false;
        }

        public void SetPassword(string password) => PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        public bool VerifyPassword(string password) => BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        public bool IsActive() => Active;
        public void Activate() => Active = true;
        public void Deactivate() => Active = false;
        public TimeSpan GetAccountAge() => DateTime.Now.Subtract(CreatedAt);
        public void UpdateProfile(string? firstName, string? lastName, string? email, string? phone)
        {
            if (!string.IsNullOrWhiteSpace(firstName) && firstName != "string")
                FirstName = firstName;
            if (!string.IsNullOrWhiteSpace(lastName) && lastName != "string")
                LastName = lastName;

            if (!string.IsNullOrWhiteSpace(email) && email != "string")
                if (new EmailAddressAttribute().IsValid(email))
                    Email = email;
                else
                    throw new ArgumentException("Invalid email format", nameof(email));

            if (!string.IsNullOrWhiteSpace(phone) && phone != "string")
                if (new RegularExpressionAttribute(@"\(?\d{2}\)?\s?\d{5}-\d{4}").IsValid(phone))
                    Phone = phone;
                else
                    throw new ArgumentException("Invalid phone format", nameof(phone));
        }
    }
}
