using Infrastructure.Repositories.AuthRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.UserDto;
using Moq;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private Mock<ILogger<AuthController>> _mockLogger;
        private Mock<IAuthRepository> _authRepositoryMock;
        private AuthController _sut;
        public AuthControllerTests()
        {
            _mockLogger = new Mock<ILogger<AuthController>>();
            _authRepositoryMock = new Mock<IAuthRepository>();

            _sut = new AuthController(_mockLogger.Object, _authRepositoryMock.Object);
        }
        #region LoginUser-Tests
        [Fact]
        public async Task LoginUser_UserNotFound_Returns_404()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _authRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.LoginUser(postUserDto);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("User Not Found", objectResult.Value);
        }

        [Fact]
        public async Task LoginUser_Wrong_Password_Returns_401()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _authRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((null, ValidationStatus.Wrong_Password));

            var result = await _sut.LoginUser(postUserDto);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, objectResult.StatusCode);
            Assert.Equal("Wrong Password", objectResult.Value);
        }

        [Fact]
        public async Task LoginUser_Authorized_Returns_JWT_And_200()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };
            var simulatedJWT = "aogduhbvoVADUVBADOVabnv3n";

            _authRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((simulatedJWT, ValidationStatus.Success));

            var result = await _sut.LoginUser(postUserDto);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(simulatedJWT, objectResult.Value);
        }

        [Fact]
        public async Task LoginUser_Exception_Returns_500()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _authRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.LoginUser(postUserDto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
    }
}
