using WalletAPI.Repositories;
using WalletAPI.Models.DTOs;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace WalletAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var secretKey = _configuration["Jwt:SecretKey"];

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret-key"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!user.VerifyPassword(password))
            {
                throw new UnauthorizedAccessException("Invalid password.");
            }

            var token = GenerateToken(user);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = email,
                Token = token
            };
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (!user.VerifyPassword(currentPassword))
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            user.SetPassword(newPassword);
            await _userRepository.UpdateAsync(user);
        }


        public async Task ValidateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty or null.");
            }

            var existingUser = await _userRepository.GetByEmailAsync(email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("This email is already in use.");
            }
        }

        public async Task<UserDto> RegisterAsync(User user, string password)
        {
            await ValidateEmailAsync(user.Email);

            user.SetPassword(password);
            var createdUSer = await _userRepository.AddAsync(user);
            var token = GenerateToken(createdUSer);

            return new UserDto
            {
                Id = createdUSer.Id,
                FirstName = createdUSer.FirstName,
                LastName = createdUSer.LastName,
                Email = user.Email,
                Token = token
            };
        }

        public async Task<UserDto> UpdateProfileAsync(int userId, string firstName, string lastName, string email, int phone)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Invalid data: First name, last name, and email are required.");
            }

            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new InvalidOperationException("Email is already in use by another user.");
            }

            user.UpdateProfile(firstName, lastName, email, phone);
            await _userRepository.UpdateAsync(user);

            var userDto = new UserDto
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Token = GenerateToken(user)
            };

            return userDto;
        }

        public async Task ActivateUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            user.Activate();
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeactivateUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            user.Deactivate();
            await _userRepository.UpdateAsync(user);
        }
    
        public async Task<TimeSpan> GetAccountAgeAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if(user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            return user.GetAccountAge();
        }

        public DateTime GetTokenExpiration()
        {
            return DateTime.Now.AddDays(1);
        }
    }
}
