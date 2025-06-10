using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Controllers.Base;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces.Repositories;
using WalletAPI.Interfaces.Services;
using WalletAPI.Models.DTOs;
using WalletAPI.Models.DTOs.Wallet;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, IUserRepository userRepository, ILogger<WalletController> logger)
            : base(walletService)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("wallet/{id}")]
        public async Task<ActionResult<WalletDto>> GetWalletById(int id)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(id);
                if (accessValidation != null) return accessValidation;

                var wallet = await _walletService.GetWalletByIdAsync(id);
                return Ok(wallet);
            }
            catch (WalletNotFoundException ex)
            {
                return NotFound($"Wallet not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetWalletId.");
                return StatusCode(500, new { message = $"Internal server error" });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequestDto request)
        {
            try
            {
                var accessValidation = ValidateUserAccess(request.UserId);
                if (accessValidation != null) return accessValidation;

                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null) return NotFound("User not found.");

                var wallet = await _walletService.CreateWalletAsync(user);
                return CreatedAtAction(
                    nameof(GetWalletById),
                    new { id = wallet.Id },
                    wallet
                );
            }
            catch (MultipleWalletsNotAllowedException)
            {
                return Conflict("User already has an active wallet.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateWallet.");
                return StatusCode(500, new { message = $"Internal server error" });

            }

        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<WalletDto>> GetWalletByUserId(int userId)
        {
            try
            {
                var accessValidation = ValidateUserAccess(userId);
                if (accessValidation != null) return accessValidation;

                var wallet = await _walletService.GetWalletByUserIdAsync(userId);
                return Ok(wallet);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet not found for user ID {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetWalletByUserId.");
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpGet("{walletId}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int walletId)
        {
            try
            {
                if (!TryGetLoggedUserId(out var userId)) return Forbid("Invalid user identity.");
                var balance = await _walletService.GetBalanceAsync(walletId, userId);
                return Ok(balance);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpPost("{walletId}/activate")]
        public async Task<ActionResult<WalletDto>> ActivateWallet(int walletId)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(walletId);
                if (accessValidation != null) return accessValidation;

                var walletDto = await _walletService.ActivateWalletAsync(walletId);
                return Ok(walletDto);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpPost("{walletId}/deactivate")]
        public async Task<ActionResult<WalletDto>> DeactivateWallet(int walletId)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(walletId);
                if (accessValidation != null) return accessValidation;

                var walletDto = await _walletService.DeactivateWalletAsync(walletId);
                return Ok(walletDto);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpDelete("{walletId}")]
        public async Task<ActionResult> DeleteWallet(int walletId)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(walletId);
                if (accessValidation != null) return accessValidation;

                await _walletService.DeleteWalletAsync(walletId);
                return NoContent();
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AllWalletsDto>>> GetAllWalletsAsync()
        {
            try
            {
                var wallets = await _walletService.GetAllWalletsAsync();
                return Ok(wallets);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }
    }
}
