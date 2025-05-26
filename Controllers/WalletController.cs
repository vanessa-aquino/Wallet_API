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

        [HttpGet("{id}")]
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
    }
}
