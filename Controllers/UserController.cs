using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Security.Claims;
using WalletAPI.Controllers.Base;
using WalletAPI.Exceptions;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Models.DTOs.User;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IWalletService walletService)
            : base(walletService)
        {
            _userService = userService;
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? throw new UnauthorizedAccessException());
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    BirthDate = dto.BirthDate,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    PasswordHash = dto.Password
                };

                var userDto = await _userService.RegisterAsync(user, dto.Password);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var userDto = await _userService.AuthenticateAsync(dto.Email, dto.Password);
                return Ok(userDto);
            }
            catch (InvalidCredentialException)
            {
                return Unauthorized("invalid credentials.");
            }
        }

        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var accesValidation = ValidateUserAccess(dto.UserId);
                if (accesValidation != null) return accesValidation;

                await _userService.ChangePasswordAsync(dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpPut("updateProfile/{userId}")]
        public async Task<ActionResult> UpdateUser([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();

                var accessValidation = ValidateUserAccess(userId);
                if (accessValidation != null) return accessValidation;

                var updateProfile = await _userService.UpdateProfileAsync(userId, dto);
                return Ok(updateProfile);
            }
            catch (EmailAlreadyInUseException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }
    }
}
