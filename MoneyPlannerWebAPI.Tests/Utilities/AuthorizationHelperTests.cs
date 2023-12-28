using MoneyPlannerWebAPI.Utilities;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Tests.Utilities
{
    public class AuthorizationHelperTests
    {
        private readonly IAuthorizationHelper _authorizationHelper;
        public AuthorizationHelperTests()
        {
            _authorizationHelper = new AuthorizationHelper();
        }

        [Fact]
        public void IsUserAuthorized_ValidUser_ReturnsTrue()
        {
            var userId = 123;
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            var result = _authorizationHelper.IsUserAuthorized(claims, userId);

            Assert.True(result);
        }

        [Fact]
        public void IsUserAuthorized_InvalidUser_ReturnsFalse()
        {
            var userId = 123;
            var invalidUserId = 456;
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, invalidUserId.ToString())
            }, "mock"));

            var result = _authorizationHelper.IsUserAuthorized(claims, userId);

            Assert.False(result);
        }       
    }
}