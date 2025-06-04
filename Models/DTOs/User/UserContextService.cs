using System.Security.Claims;
using WalletAPI.Interfaces.Services;

namespace WalletAPI.Models.DTOs.User
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            var userIdClaims = _httpContextAccessor.HttpContext?.User?
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaims == null)
                throw new UnauthorizedAccessException("User Id claim not found.");
        
            return int.Parse(userIdClaims.Value);
        }
    }
}
