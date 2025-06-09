using WalletAPI.Interfaces.Repositories;
using WalletAPI.Interfaces.Services;
using WalletAPI.Models;
using WalletAPI.Models.DTOs.PasswordReset;

namespace WalletAPI.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserService _userService;
        private readonly IPasswordResetRepository _passwordResetRepository;

        public PasswordResetService(IUserService userService, IPasswordResetRepository passwordResetRepository)
        {
            _userService = userService;
            _passwordResetRepository = passwordResetRepository;
        }

        public async Task<string> GenerateResetTokenAsync(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            string token = Guid.NewGuid().ToString();

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
            };

            await _passwordResetRepository.AddAsync(resetToken);

            return token; // Retorno do token para não precisar enviar o email, visto que os testes são feitas com email inexistentes
        }

        public async Task<PasswordResetToken> ValidateResetTokenAsync(string token)
        {
            var resetToken = await _passwordResetRepository.GetByTokenAsync(token);

            if (resetToken == null)
                throw new InvalidOperationException("Invalid token.");

            if (resetToken.ExpirationDate <= DateTime.Now)
                throw new InvalidOperationException("Expired token.");

            if (resetToken.Used)
                throw new InvalidOperationException("Token has already been used.");

            return resetToken;
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await ValidateResetTokenAsync(token);

            var user = resetToken.User;
            user.SetPassword(newPassword);
            resetToken.MarkUsed();
            
            await _userService.UpdateAsync(user);
            await _passwordResetRepository.UpdateAsync(resetToken);
        }

    }
}
