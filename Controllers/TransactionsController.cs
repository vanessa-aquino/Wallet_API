using Microsoft.AspNetCore.Authorization;
using WalletAPI.Models.DTOs.Transaction;
using WalletAPI.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using WalletAPI.Models.DTOs;
using WalletAPI.Models;
using WalletAPI.Controllers.Base;
using WalletAPI.Exceptions;
using WalletAPI.Models.Enums;
using System.Diagnostics;

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
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
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
                var accessValidation = await ValidateWalletAccessAsync(dto.WalletId);
                if (accessValidation != null) return accessValidation;

                if (!TryGetLoggedUserId(out var loggedUserId))
                    return StatusCode(403, "Invalid user identity");

                var transaction = await _transactionService.DepositAsync(dto, loggedUserId);
                var responseDto = new ResponseDepositAndWithdrawDto
                {
                    TransactionType = transaction.TransactionType,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    WalletName = $"{transaction.User.FirstName} {transaction.User.LastName}",
                    Description = transaction.Description
                };

                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, responseDto);
            }
            catch (InvalidTransactionException)
            {
                return BadRequest("Invalid transaction data.");
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
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawAndDepositTransactionDto dto)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(dto.WalletId);
                if (accessValidation != null) return accessValidation;


                if (!TryGetLoggedUserId(out var loggedUserId))
                    return StatusCode(403, "Invalid user identity");

                var transaction = await _transactionService.WithdrawAsync(dto, loggedUserId);
                var responseDto = new ResponseDepositAndWithdrawDto
                {
                    TransactionType = transaction.TransactionType,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    WalletName = $"{transaction.User.FirstName} {transaction.User.LastName}",
                    Description = transaction.Description
                };

                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, responseDto);
            }
            catch (InvalidTransactionException)
            {
                return BadRequest();
            }
            catch (InsufficientFundsException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedTransactionException)
            {
                return Forbid();
            }
            catch (NotFoundException)
            {
                return NotFound($"Wallet with id {dto.WalletId} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferTransactionDto dto)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(dto.SourceWalletId);
                if (accessValidation != null) return accessValidation;

                if (!TryGetLoggedUserId(out var loggedUserId))
                    return StatusCode(403, "Invalid user identity.");

                var transaction = await _transactionService.TransferAsync(dto, loggedUserId);
                var responseDto = new ResponseTransferDto
                {
                    TransactionType = transaction.TransactionType,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    SourceWalletOwnerName = $"{transaction.User?.FirstName} {transaction.User?.LastName}",
                    DestinationWalletOwnerName = $"{transaction.DestinationWallet?.User?.FirstName ?? ""} {transaction.DestinationWallet?.User?.LastName ?? ""} ",
                    Description = transaction.Description
                };

                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, responseDto);
            }
            catch (InvalidTransactionException)
            {
                return BadRequest();
            }
            catch (InsufficientFundsException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedTransactionException)
            {
                return Forbid();
            }
            catch (NotFoundException)
            {
                return NotFound($"Wallet with id {dto.SourceWalletId} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }


        [HttpGet("report")]
        public async Task<IActionResult> TransactionReport([FromQuery] int walletId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var accessValidation = await ValidateWalletAccessAsync(walletId);
                if (accessValidation != null) return Forbid();

                var csvByte = await _transactionService.GenerateTransactionReportAsync(walletId, startDate, endDate);
                var fileName = $"Relatorio_transacoes_{walletId}_{DateTime.Now:ddMMyyyy}.csv";
                return File(csvByte, "text/csv", fileName);
            }
            catch (WalletNotFoundException)
            {
                return NotFound("Wallet not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> TransactionHistory([FromQuery] TransactionFilterDto dto)
        {
            try
            {
                if (dto.WalletId <= 0)
                    throw new ArgumentException("WalletId is required.");

                var accessValidation = await ValidateWalletAccessAsync(dto.WalletId);
                if (accessValidation != null) return accessValidation;

                var transactionList = await _transactionService.GetTransactionHistoryAsync(dto);
                return Ok(transactionList);
            }
            catch (ArgumentException)
            {
                return BadRequest("WalletId is required.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpGet("admin/history")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<IEnumerable<Transaction>>> AdminTransactionHistory([FromQuery] TransactionFilterDto dto)
        {
            try
            {
                if (dto.WalletId <= 0)
                {
                    var allTransactions = await _transactionService.GetAllAsync();
                    return Ok(allTransactions);
                }

                var transactions = await _transactionService.GetTransactionHistoryAsync(dto);
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

        [HttpPost("revert/{transactionId}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> RevertTransactions(int transactionId)
        {
            try
            {
                await _transactionService.RevertTransactionAsync(transactionId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound(transactionId);
            }
            catch (TransactionCannotBeReversedException)
            {
                return Conflict("This transaction cannot be reversed");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpGet("total/{walletId}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> NumberOfTransactions(int walletId)
        {
            try
            {
                var number = await _transactionService.GetTotalTransactionsAsync(walletId);
                return Ok(number);
            }
            catch(WalletNotFoundException)
            {
                return NotFound("Wallet not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}

