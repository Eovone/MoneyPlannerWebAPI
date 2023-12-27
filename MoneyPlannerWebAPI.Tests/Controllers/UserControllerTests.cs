using AutoMapper;
using Entity;
using Infrastructure.Repositories.UserRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.UserDto;
using MoneyPlannerWebAPI.Utilities;
using Moq;
using System.Security.Claims;
using System.Text;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class UserControllerTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ILogger<UserController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private UserController _sut;
        private Mock<IAuthorizationHelper> _authorizationHelperMock;
        public UserControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockMapper = new Mock<IMapper>();
            _authorizationHelperMock = new Mock<IAuthorizationHelper>();

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(true);

            _sut = new UserController(_mockMapper.Object, _userRepositoryMock.Object, _mockLogger.Object, _authorizationHelperMock.Object);
        }
        #region CreateUser-Tests 
        [Fact]
        public async Task CreateUser_Invalid_Password_Returns_400()
        {
            var postUserDto = new PostUserDto { Username = "testUser" , Password = "testPassword"};
            
            _userRepositoryMock.Setup(x => x.AddUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((null, ValidationStatus.Invalid_Password));

            var result = await _sut.CreateUser(postUserDto);

            Assert.IsType<BadRequestObjectResult>(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Password", objectResult.Value);
        }

        [Fact]
        public async Task CreateUser_UserName_Already_Exist_Returns_400()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _userRepositoryMock.Setup(x => x.AddUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((null, ValidationStatus.Username_Already_Exist));

            var result = await _sut.CreateUser(postUserDto);

            Assert.IsType<BadRequestObjectResult>(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Username_Already_Exist", objectResult.Value);
        }

        [Fact]
        public async Task CreateUser_Valid_Dto_Returns_201()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" }; 
            var user = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            user.Id = 1;

            _userRepositoryMock.Setup(x => x.AddUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((user, ValidationStatus.Success)); 

            var result = await _sut.CreateUser(postUserDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task CreateUser_Exception_Returns_500()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _userRepositoryMock.Setup(x => x.AddUser(postUserDto.Username, postUserDto.Password))
                               .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.CreateUser(postUserDto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUser-Tests
        [Fact]
        public async Task GetUser_UnauthorizedUser_Returns_401()
        {
            int userId = 1;
            int unauthorizedUserId = 2;
            var user = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            user.Id = 1;            

            _userRepositoryMock.Setup(x => x.GetUser(userId))
                               .ReturnsAsync(user);

            _mockMapper.Setup(x => x.Map<GetUserDto>(user))
                       .Returns(new GetUserDto());

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(false);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, unauthorizedUserId.ToString()),
                new Claim(ClaimTypes.Name, "SomeUsername"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity!.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Claims).Returns(claims);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            var result = await _sut.GetUser(userId);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }
        [Fact]
        public async Task GetUser_UserNotFound_Returns_404()
        {
            _userRepositoryMock.Setup(x => x.GetUser(1))
                               .ReturnsAsync(null as User);

            var result = await _sut.GetUser(1);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("User Not Found", objectResult.Value);
        }

        [Fact]
        public async Task GetUser_UserFound_Returns_200()
        {
            var user = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            user.Id = 1;

            _userRepositoryMock.Setup(x => x.GetUser(1))
                               .ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<GetUserDto>(user))
                       .Returns(new GetUserDto { Id = 1, Username = "testUser", Incomes = new List<Income> { }, Expenses = new List<Expense> { } });

            var result = await _sut.GetUser(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetUserDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetUser_Exception_Returns_500()
        {
            _userRepositoryMock.Setup(x => x.GetUser(1))
                               .ThrowsAsync(new Exception("Simulated exception"));            

            var result = await _sut.GetUser(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion 
    }
}