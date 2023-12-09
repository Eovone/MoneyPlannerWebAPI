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
    }
}
