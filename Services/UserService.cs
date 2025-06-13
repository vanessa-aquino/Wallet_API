﻿using WalletAPI.Interfaces.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using WalletAPI.Interfaces.Services;
using WalletAPI.Models.DTOs.User;
using WalletAPI.Models.Enums;
using System.Security.Claims;
using WalletAPI.Exceptions;
using WalletAPI.Models;
using System.Text;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserService> _logger;
        private const string UserCacheKey = "User_";

        public UserService(IUserRepository userRepository, IConfiguration configuration, IMemoryCache cache, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }

        public async Task UpdateAsync(User user) => await _userRepository.UpdateAsync(user);
        public DateTime GetTokenExpiration() => DateTime.Now.AddDays(1);
        public string GenerateToken(User user)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var secretKey = _configuration["Jwt:SecretKey"];

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = GetTokenExpiration();

            var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> GetUserById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user;
        }

        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            var cacheKey = $"{UserCacheKey}{email}";

            if (!_cache.TryGetValue(cacheKey, out User user))
            {
                user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning($"Failed login attempt for email : {email}");
                    throw new InvalidUserCredentialsException();
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, user, cacheOptions);
            }

            if (!user.VerifyPassword(password))
            {
                _logger.LogWarning($"Failed login attempt for email : {email}");
                throw new InvalidUserCredentialsException();
            }

            var token = GenerateToken(user);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                Email = email,
                Phone = user.Phone,
                Role = user.Role
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"Attempted password change for non-existent user ID {userId}");
                throw new UserNotFoundException(userId);
            }

            if (!user.VerifyPassword(dto.CurrentPassword))
            {
                _logger.LogWarning($"Incorrect current password for user ID {userId}");
                throw new InvalidUserCredentialsException();
            }

            user.SetPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            var cacheKey = $"{UserCacheKey}{user.Email}";
            _cache.Remove(cacheKey);

            _logger.LogInformation($"Password changed successfully for user ID {userId}");
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
                _logger.LogWarning($"Attempt to register with already in use email: {email}");
                throw new EmailAlreadyInUseException(email);
            }
        }

        public async Task<UserDto> RegisterAsync(User user, string password)
        {
            await ValidateEmailAsync(user.Email);
            user.SetPassword(password);
            user.Role = user.Email.Contains("admin@email.com") ? UserRole.Admin : UserRole.User;

            var createdUser = await _userRepository.AddAsync(user);

            _logger.LogInformation($"New user registered with ID {createdUser.Id}");
            var token = GenerateToken(createdUser);

            return new UserDto
            {
                Id = createdUser.Id,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Email = createdUser.Email,
                Phone = createdUser.Phone,
                Token = token,
                Role = createdUser.Role
            };
        }

        public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Id != userId)
                    throw new EmailAlreadyInUseException(dto.Email);
            }

            user.UpdateProfile(dto.FirstName, dto.LastName, dto.Email, dto.Phone);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation($"User profile updated for user ID {userId}");

            var cacheKey = $"{UserCacheKey}{user.Email}";
            _cache.Remove(cacheKey);
            _cache.Set(cacheKey, user, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });

            return new UserDto
            {
                Id = userId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Token = GenerateToken(user)
            };
        }

        public async Task ActivateUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Activate();
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeactivateUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Deactivate();
            await _userRepository.UpdateAsync(user);
        }

        public async Task<TimeSpan> GetAccountAgeAsync(int userId)
        {
            var cacheKey = $"{UserCacheKey}AccountAge_{userId}";

            if (!_cache.TryGetValue(cacheKey, out TimeSpan accountAge))
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserNotFoundException(userId);
                }

                accountAge = user.GetAccountAge();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

                _cache.Set(cacheKey, accountAge, cacheOptions);
            }

            return accountAge;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new UserNotFoundException($"User with email {email} not found.");
            return user;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            await _userRepository.DeleteAsync(userId);
            _logger.LogInformation($"User with Id {userId} deleted.");
        }

        public async Task<PagedResultDto<UserProfileDto>> PaginationAsync(UserQueryParams pagination)
        {
            return await _userRepository.PaginationAsync(pagination);
        }
    }
}
