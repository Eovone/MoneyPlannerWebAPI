using Entity;
using Infrastructure.Repositories.AuthRepo;
using Infrastructure.Utilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories.AuthRepo
{
    public class AuthRepositoryTests
    {
        private Mock<DataContext> _mockDataContext;
        private List<User> _userList;
        private Mock<IConfiguration> _mockConfig;
        private AuthRepository _sut;
        public AuthRepositoryTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _userList = SeedUsers();
            _mockDataContext.Setup(x => x.Users)
                            .ReturnsDbSet(_userList);

            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("amdvpiADVJEHVAEPVpeaivhnapeAIEHVp1h3fihfPEIQpi1vhqei");
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(x => x.GetSection("Jwt:Token")).Returns(mockConfigSection.Object);

            _sut = new AuthRepository(_mockDataContext.Object, _mockConfig.Object);
        }
        #region Private Seeding
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
        #endregion
        #region LoginUser-Tests
        [Fact]
        public async Task LoginUser_User_Not_Found_Returns_Null_And_ValidStatus_Not_Found()
        {
            string testUsername = "TestUser123";
            string testPassword = "testPass123";

            var (authResult, validationStatus) = await _sut.LoginUser(testUsername, testPassword);

            Assert.Null(authResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Fact]
        public async Task LoginUser_Wrong_Password_Returns_Null_And_ValidStatus_Wrong_Password()
        {
            string testUsername = "TestUser1";
            string testPassword = "testPass123";

            var (authResult, validationStatus) = await _sut.LoginUser(testUsername, testPassword);

            Assert.Null(authResult);
            Assert.Equal(ValidationStatus.Wrong_Password, validationStatus);
        }

        [Fact]
        public async Task LoginUser_Authorized_Returns_JWT_And_ValidStatus_Success()
        {
            string testUsername = "TestUser1";
            string testPassword = "Password1!";

            var (authResult, validationStatus) = await _sut.LoginUser(testUsername, testPassword);

            Assert.NotNull(authResult);
            Assert.Equal(ValidationStatus.Success, validationStatus);
            Assert.IsType<string>(authResult);
        }
        #endregion
    }
}
