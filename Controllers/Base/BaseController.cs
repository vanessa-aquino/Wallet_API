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
            if (!TryGetLoggedUserId(out var userId)) return Forbid("Invalid user identity");

            var hasAccess = await _walletService.HasAccessAsync(walletId, userId, User);
            if (!hasAccess) return Forbid("You do not have access to this wallet.");

            return null;
        }

        protected ActionResult? ValidateUserAccess(int targetUserId)
        {
            if (!TryGetLoggedUserId(out var loggedUserId))
                return Forbid("Invalid user identity");
            if (!_walletService.HasAccessToUser(targetUserId, loggedUserId, User))
                return Forbid("You do not have permission to access this user.");

            return null;
        }
    }
}
