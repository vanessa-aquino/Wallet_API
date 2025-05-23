using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Models.DTOs;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            Console.WriteLine("Entrou no Register");
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
                return BadRequest( new { message = ex.Message});
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var userDto = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
                return Ok(userDto);
            }
            catch (InvalidCredentialException)
            {
                return Unauthorized("invalid credentials.");
            }
        }
    }
}
