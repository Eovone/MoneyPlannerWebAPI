using AutoMapper;
using Entity;
using Infrastructure.Repositories.UserRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.UserDto;
using Moq;
using System.Text;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class UserControllerTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ILogger<UserController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private UserController _sut;
        public UserControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockMapper = new Mock<IMapper>();

            _sut = new UserController(_mockMapper.Object, _userRepositoryMock.Object, _mockLogger.Object);
        }
        #region CreateUser-Tests
        [Fact]
        public async Task CreateUser_Invalid_Password_Returns_400()
        {
            var postUserDto = new PostUserDto { Username = "testUser" , Password = "testPassword"};
            
            _userRepositoryMock.Setup(x => x.AddUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((null, ValidationStatus.Invalid_Password));

            var result = await _sut.CreateUser(postUserDto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
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

            Assert.IsType<BadRequestObjectResult>(result.Result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
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
           
            _mockMapper.Setup(x => x.Map<GetUserDto>(user))
                       .Returns(new GetUserDto());

            var result = await _sut.CreateUser(postUserDto);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.NotNull(createdAtResult.Value);
            Assert.IsType<GetUserDto>(createdAtResult.Value);
        }

        [Fact]
        public async Task CreateUser_Exception_Returns_500()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _userRepositoryMock.Setup(x => x.AddUser(postUserDto.Username, postUserDto.Password))
                               .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.CreateUser(postUserDto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUser-Tests
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
                       .Returns(new GetUserDto { Incomes = new List<Income> { }, Expenses = new List<Expense> { } });

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
        #region Login-Tests
        [Fact]
        public async Task LoginUser_User_NotFound_Returns_404()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };
            _userRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
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
            _userRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((null, ValidationStatus.Wrong_Password));

            var result = await _sut.LoginUser(postUserDto);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, objectResult.StatusCode);
            Assert.Equal("Wrong Password", objectResult.Value);
        }

        [Fact]
        public async Task LoginUser_Successfully_Returns_200()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };
            var loginDto = new LoginDto { Id = 1, IsAuthorized = true };

            _userRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ReturnsAsync((loginDto, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<GetLoginUserDto>(loginDto))
                       .Returns(new GetLoginUserDto());

            var result = await _sut.LoginUser(postUserDto);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetLoginUserDto>(objectResult.Value);
        }

        [Fact]
        public async Task LoginUser_Exception_Returns_500()
        {
            var postUserDto = new PostUserDto { Username = "testUser", Password = "testPassword" };

            _userRepositoryMock.Setup(x => x.LoginUser(postUserDto.Username, postUserDto.Password))
                               .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.LoginUser(postUserDto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
    }
}
