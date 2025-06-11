using Microsoft.AspNetCore.Authorization;
using WalletAPI.Models.DTOs.Transaction;
using WalletAPI.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Models.DTOs;
using WalletAPI.Models;
using WalletAPI.Controllers.Base;
using WalletAPI.Exceptions;

namespace WalletAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(IWalletService walletService, ITransactionService transactionService)
            : base(walletService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);
                return Ok(transaction);
            }
            catch(NotFoundException)
            {
                return NotFound("Transaction not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] WithdrawAndDepositTransactionDto dto)
        {
            try
            {
                var userId = ValidateUserAccess(dto.UserId);
                var transaction = await _transactionService.DepositAsync(dto);

                var responseDto = new ResponseDepositAndWithdrawDto
                {
                    TransactionType = transaction.TransactionType,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    Description = transaction.Description,
                    WalletName = $"{transaction.User.FirstName} {transaction.User.LastName}"
                };

                return CreatedAtAction(nameof(GetById), new {id = transaction.Id}, responseDto);
            }
            catch (InvalidTransactionException)
            {
                return BadRequest($"Invalid transaction data.");
            }
            catch (UnauthorizedTransactionException)
            {
                return Forbid();
            }
            catch (NotFoundException)
            {
                return NotFound($"Wallet with id {dto.WalletId} not found");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"Internal server erro." });
            }
        }

            //[HttpGet]
            //public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions([FromQuery] TransactionFilterDto filterDto)
            //{
            //    try
            //    {

            //        var transactions = await _transactionService.GetTransactionHistoryAsync(filterDto);
            //        return Ok(transactions);
            //    }
            //    catch (KeyNotFoundException ex)
            //    {
            //        return NotFound(ex.Message);
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, $"Internal server error: {ex.Message}");
            //    }
            //}

            //[HttpGet("{id}")]
            //public async Task<ActionResult<TransactionDto>> GetTransactionById(int id)
            //{
            //    try
            //    {
            //        var transaction = await _transactionService.GetByIdAsync(id);
            //        return Ok(transaction);
            //    }
            //    catch (KeyNotFoundException)
            //    {
            //        return NotFound($"Transactions with ID {id} not found.");
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, $"Internal server error: {ex.Message}");
            //    }
            //}






        }
    }
