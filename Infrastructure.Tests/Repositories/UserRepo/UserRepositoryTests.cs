using Entity;
using Infrastructure.Repositories.UserRepo;
using Infrastructure.Utilities;
using Moq;
using Moq.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories.UserRepo
{
    public class UserRepositoryTests
    {
        private Mock<DataContext> _mockDataContext;
        private List<User> _userList;
        private UserRepository _sut;
        public UserRepositoryTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _userList = SeedUsers();
            _mockDataContext.Setup(x => x.Users)
                            .ReturnsDbSet(_userList);
            _sut = new UserRepository(_mockDataContext.Object);
        }
        private static List<User> SeedUsers()
        {
            var userList = new List<User>();

            for (int i = 1; i < 6; i++)
            {
                var salt = PasswordHasher.GenerateSalt();
                var hash = PasswordHasher.HashPassword($"Password{i}!", salt);
                userList.Add(new User($"TestUser{i}", salt, hash)
                { Id = i });
            }
            return userList;
        }
        #region AddUser-Tests
        [Theory]
        [InlineData("")]
        [InlineData("asd")]
        [InlineData("asdfgAAAd")]
        [InlineData("asdfgevqe123")]
        [InlineData("ASDFG1234")]
        public async Task AddUser_InvalidPassword_Returns_NullObject_And_ValidStatus_Invalid_Password(string password)
        {
            string username = "TestUser123";

            var (userResult, validationStatus) = await _sut.AddUser(username, password);

            Assert.Null(userResult);
            Assert.Equal(ValidationStatus.Invalid_Password, validationStatus);
        }

        [Fact]
        public async Task AddUser_Username_Already_Exist_Returns_NullObject_And_ValidStatus_Username_Already_Exist()
        {
            string username = "TestUser1";

            var (userResult, validationStatus) = await _sut.AddUser(username, "TestPass123");

            Assert.Null(userResult);
            Assert.Equal(ValidationStatus.Username_Already_Exist, validationStatus);
        }

        [Fact]
        public async Task AddUser_Successfully_Returns_User_And_ValidStatus_Success()
        {
            string username = "TestUser123";

            var (userResult, validationStatus) = await _sut.AddUser(username, "TestPass123");

            Assert.NotNull(userResult);
            Assert.Equal(username, userResult.Username);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
        #region GetUser-Tests
        [Fact]
        public async Task GetUser_User_NotFound_Returns_Null()
        {
            int userId = 20;

            var userResult = await _sut.GetUser(userId);

            Assert.Null(userResult);            
        }

        [Fact]
        public async Task GetUser_User_Successfully_Returns_UserObject()
        {
            int userId = 1;
            _mockDataContext.Setup(x => x.Users.FindAsync(userId))
                            .ReturnsAsync(_userList.FirstOrDefault(u => u.Id == userId));

            var userResult = await _sut.GetUser(userId);

            Assert.NotNull(userResult);
            Assert.Equal("TestUser1", userResult.Username);
            _mockDataContext.Verify(x => x.Users, Times.Once);
        }
        #endregion
        #region LoginUser-Tests
        [Fact]
        public async Task LoginUser_User_NotFound_Returns_NullObject_And_ValidStatus_NotFound()
        {
            string username = "TestUser1234";
            string password = "Password1!";           

            var (userResult, validationStatus) = await _sut.LoginUser(username, password);

            Assert.Null(userResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Fact]
        public async Task LoginUser_Input_WrongPassword_Returns_NullObject_And_ValidStatus_Wrong_Password()
        {
            string username = "TestUser1";
            string password = "Password16533t1";

            var (userResult, validationStatus) = await _sut.LoginUser(username, password);

            Assert.Null(userResult);
            Assert.Equal(ValidationStatus.Wrong_Password, validationStatus);
        }

        [Fact]
        public async Task LoginUser_Input_Correct_Returns_LoginDto_And_ValidStatus_Success()
        {
            string username = "TestUser1";
            string password = "Password1!";

            var (userResult, validationStatus) = await _sut.LoginUser(username, password);

            Assert.NotNull(userResult);
            Assert.Equal(ValidationStatus.Success, validationStatus);
            Assert.Equal(1, userResult.Id);
            Assert.True(userResult.IsAuthorized);
        }
        #endregion
    }
}
