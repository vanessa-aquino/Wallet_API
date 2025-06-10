using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletAPI.Interfaces.Services;

namespace WalletAPI.Controllers.Base
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IWalletService _walletService;

        protected BaseController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        protected bool TryGetLoggedUserId(out int userId)
        {
            userId = 0;
            var useridStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(useridStr) && int.TryParse(useridStr, out userId);
        }

        protected async Task<ActionResult?> ValidateWalletAccessAsync(int walletId)
        {
            if (!TryGetLoggedUserId(out var userId))
                return StatusCode(403, "Invalid user identity.");

            Console.WriteLine($"Validando acesso do wallet {walletId} para user {userId}");
            var hasAccess = await _walletService.HasAccessAsync(walletId, userId, User);
            if (!hasAccess)
                return StatusCode(403, "You do not have permission to access this wallet.");

            return null;
        }

        protected ActionResult? ValidateUserAccess(int targetUserId)
        {
            if (!TryGetLoggedUserId(out var loggedUserId))
                return StatusCode(403, "Invalid user identity");

            if (!_walletService.HasAccessToUser(targetUserId, loggedUserId, User))
                return StatusCode(403, "You do not have permission to access this wallet.");

            return null;
        }
    }
}
