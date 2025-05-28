using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IUserRepository _userRepository;

        public WalletController(IWalletService walletService, IUserRepository userRepository)
        {
            _walletService = walletService;
            _userRepository = userRepository;
        }

        private bool TryGetLoggedUserId(out int userId)
        {
            userId = 0;
            var useridStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(useridStr) && int.TryParse(useridStr, out userId);
        }

        [HttpGet("wallet/{id}")]
        public async Task<ActionResult<WalletDto>> GetWalletById(int id)
        {
            try
            {
                if (!TryGetLoggedUserId(out var userId)) return Forbid("Invalid user identity.");
                var hasAccess = await _walletService.HasAccessAsync(id, userId, User);
                if (!hasAccess) return Forbid("You do not have access to this wallet.");

                var wallet = await _walletService.GetWalletByIdAsync(id);
                return Ok(wallet);
            }
            catch (WalletNotFoundException ex)
            {
                return NotFound($"Wallet not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequestDto request)
        {
            try
            {
                if(!TryGetLoggedUserId(out var loggedUserId)) return Forbid("Invalid user identity.");
                if (!_walletService.HasAccessToUser(request.UserId, loggedUserId, User))
                    return Forbid("You do not have permission to create a wallet for this user.");

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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<WalletDto>> FindWalletByUserId(int userId)
        {
            try
            {
                if (!TryGetLoggedUserId(out var loggedUserId)) return Forbid("Invalid user identity.");
                if (!_walletService.HasAccessToUser(userId, loggedUserId, User))
                    return Forbid("You do not have permission to create a wallet for this user.");

                var wallet = await _walletService.GetWalletByUserIdAsync(userId);
                return Ok(wallet);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet not found for user ID {userId}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{walletId}/activate")]
        public async Task<ActionResult<WalletDto>> ActivateWallet(int walletId)
        {
            try
            {
                if (!TryGetLoggedUserId(out var userId)) return Forbid("Invalid user identity.");
                var hasAccess = await _walletService.HasAccessAsync(walletId, userId, User);
                if (!hasAccess) return Forbid("You do not have access to this wallet.");

                var walletDto = await _walletService.ActivateWalletAsync(walletId);
                return Ok(walletDto);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{walletId}/deactivate")]
        public async Task<ActionResult<WalletDto>> DeactivateWallet(int walletId)
        {
            try
            {
                if (!TryGetLoggedUserId(out var userId)) return Forbid("Invalid user identity.");
                var hasAccess = await _walletService.HasAccessAsync(walletId, userId, User);
                if (!hasAccess) return Forbid("You do not have access to this wallet.");

                var walletDto = await _walletService.DeactivateWalletAsync(walletId);
                return Ok(walletDto);
            }
            catch (WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
