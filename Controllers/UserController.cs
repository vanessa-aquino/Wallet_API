using Microsoft.AspNetCore.Authorization;
using System.Security.Authentication;
using WalletAPI.Models.DTOs.User;
using WalletAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Exceptions;
using WalletAPI.Models;
using WalletAPI.Interfaces.Services;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IUserContextService _userContextService;

        public UserController(IUserService userService, IUserContextService userContextService, IWalletService walletService)
            : base(walletService)
        {
            _userService = userService;
            _userContextService = userContextService;
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
                return Unauthorized("Invalid credentials.");
            }
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> MyProfile()
        {
            try
            {
                var userId = _userContextService.GetUserId();
                var user = await _userService.GetUserById(userId);

                var userDto = new UserProfileDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Email = user.Email,
                    Active = user.Active,
                    BirthDate = user.BirthDate,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userDto);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidUserCredentialsException)
            {
                return Unauthorized("Invalid credentials");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(int userId, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                userId = _userContextService.GetUserId();

                var accesValidation = ValidateUserAccess(userId);
                if (accesValidation != null) return accesValidation;

                await _userService.ChangePasswordAsync(userId, dto);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidUserCredentialsException)
            {
                return Unauthorized("Invalid credentials");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });

            }
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult> UpdateUser([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = _userContextService.GetUserId();

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

        [HttpGet("account-age")]
        public async Task<ActionResult<TimeSpan>> GetAccountAge(int userId)
        {
            try
            {
                var accessValidation = ValidateUserAccess(userId);
                if (accessValidation != null) return accessValidation;

                var accountAge = await _userService.GetAccountAgeAsync(userId);
                return Ok(accountAge.Days);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            try
            {
                var accessValidation = ValidateUserAccess(userId);
                if (accessValidation != null) return accessValidation;

                await _userService.DeleteUserAsync(userId);
                return NoContent();
            }
            catch (UserNotFoundException)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            catch(Exception)
            {
                return StatusCode(500, new { message = $"Internal server error" });
            }
        }

        [HttpPut("{userId}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ActivatedUser(int userId)
        {
            try
            {
                await _userService.ActivateUserAsync(userId);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{userId}/deactive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivatedUser(int userId)
        {
            try
            {
                await _userService.DeactivateUserAsync(userId);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }




    }
}
