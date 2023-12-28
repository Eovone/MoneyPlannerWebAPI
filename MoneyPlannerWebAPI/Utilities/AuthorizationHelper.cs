using System.Security.Claims;

namespace MoneyPlannerWebAPI.Utilities
{    public interface IAuthorizationHelper
    {
        bool IsUserAuthorized(ClaimsPrincipal user, int userId);
    }
    public class AuthorizationHelper : IAuthorizationHelper
    {
        public bool IsUserAuthorized(ClaimsPrincipal user, int userId)
        {
            var authenticatedUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return authenticatedUserId == userId;
        }
    }
}