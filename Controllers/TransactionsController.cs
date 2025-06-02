using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Models.DTOs;
using WalletAPI.Interfaces;
using WalletAPI.Models;
using WalletAPI.Models.DTOs.Transaction;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions([FromQuery] TransactionFilterDto filterDto)
        {
            try
            {

                var transactions = await _transactionService.GetTransactionHistoryAsync(filterDto);
                return Ok(transactions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> GetTransactionById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Transactions with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





        //[HttpPost("deposit")]
        //public async Task<ActionResult<Transaction>> Deposit([FromBody] WithdrawAndDepositTransactionDto dto)
        //{
        //    if(!ModelState.IsValid) return BadRequest(ModelState);

        //    try
        //    {
        //        var transaction = await _transactionService.DepositAsync(dto);
        //        return CreatedAtAction(nameof(Deposit), new {id = transaction.Id}, transaction);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest( new { error = ex.Message });
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, "Internal error processing the deposit;");
        //    }
        //}
    }
}
