using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Models.DTOs;
using WalletAPI.Interfaces;
using System.Security.Claims;
using WalletAPI.Models;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("deposit")]
        public async Task<ActionResult<Transaction>> Deposit([FromBody] WithdrawAndDepositTransactionDto dto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                var transaction = await _transactionService.DepositAsync(dto);
                return CreatedAtAction(nameof(Deposit), new {id = transaction.Id}, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest( new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal error processing the deposit;");
            }
        }
    }
}
