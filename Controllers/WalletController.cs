using Microsoft.AspNetCore.Mvc;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IUserRepository _userRepository;

        public WalletController(IWalletService walletService, IUserRepository userRepository)
        {
            _walletService = walletService;
            _userRepository = userRepository;
        }

        [HttpGet("wallet/{id}")]
        public async Task<ActionResult<WalletDto>> GetWalletById(int id)
        {
            try
            {
                var wallet = await _walletService.GetWalletByIdAsync(id);
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return NotFound($"Wallet not found: {ex.Message}");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequestDto request)
        {
            try
            {
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
                var wallet = await _walletService.GetWalletByUserIdAsync(userId);
                return Ok(wallet);
            }
            catch(WalletNotFoundException)
            {
               return NotFound($"Wallet not found for user ID {userId}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{walletId}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int walletId)
        {
            try
            {
                var balance = await _walletService.GetBalanceAsync(walletId);
                return Ok(balance);
            }
            catch(WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{walletId}/activate")]
        public async Task<ActionResult> ActivateWallet(int walletId)
        {
            try
            {
                await _walletService.ActivateWalletAsync(walletId);
                return NoContent();
            }
            catch(WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{walletId}/deactivate")]
        public async Task<ActionResult> DeactivateWallet(int walletId)
        {
            try
            {
                await _walletService.DeactivateWalletAsync(walletId);
                return NoContent();
            }
            catch(WalletNotFoundException)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
