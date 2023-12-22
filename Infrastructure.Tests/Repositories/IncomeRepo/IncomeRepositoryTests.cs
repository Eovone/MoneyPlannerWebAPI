using Entity;
using Infrastructure.Repositories.IncomeRepo;
using Infrastructure.Utilities;
using Moq;
using Moq.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories.IncomeRepo
{
    public class IncomeRepositoryTests
    {
        private Mock<DataContext> _mockDataContext;
        private List<User> _userList;
        private List<Income> _incomeList;
        private IncomeRepository _sut;
        public IncomeRepositoryTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _userList = SeedUsers();
            _incomeList = SeedIncomes();
            _mockDataContext.Setup(x => x.Users)
                            .ReturnsDbSet(_userList);
            _mockDataContext.Setup(x => x.Incomes)
                            .ReturnsDbSet(_incomeList); 
            _sut = new IncomeRepository(_mockDataContext.Object);
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
        private static List<Income> SeedIncomes()
        {
            var users = SeedUsers();
            var incomeList = new List<Income>();

            for (int i = 1; i < 6; i++)
            {
                incomeList.Add(new Income($"TestTitle{i}", i, new DateTime(2023, i, 5))
                { Id = i, ReOccuring = false, User = users[i - 1] });
            }
            return incomeList;
        }
        #endregion
        #region AddIncome-Tests
        [Fact]
        public async Task AddIncome_User_NotFound_Returns_Null_And_ValidationStatus_Not_Found()
        {
            var userIdThatDoesntExist = 10;
            var income = new Income("incomeTest", 20, new DateTime());

            var (incomeResult, validationStatus) = await _sut.AddIncome(income, userIdThatDoesntExist);

            Assert.Null(incomeResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("posafvhopsuvhaopsfhvsopafuvhasfuopvhasfpuiovhapfuiovhasuiopfvhapsdfuiovhafpsduiovh")]
        public async Task AddIncome_Income_Title_Wrong_Size_Returns_Null_And_ValidationStatus_Invalid_Amount_Of_Chars(string incomeTitle)
        {
            var userIdThatExist = 1;
            var income = new Income(incomeTitle, 20, new DateTime());

            var (incomeResult, validationStatus) = await _sut.AddIncome(income, userIdThatExist);

            Assert.Null(incomeResult);
            Assert.Equal(ValidationStatus.Invalid_Amount_Of_Characters, validationStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(99999999999999)]
        public async Task AddIncome_Income_Amount_Is_Invalid_Returns_Null_And_ValidationStatus_Invalid_Amount(double incomeAmount)
        {
            var userIdThatExist = 1;
            var income = new Income("testTitle", incomeAmount, new DateTime());

            var (incomeResult, validationStatus) = await _sut.AddIncome(income, userIdThatExist);

            Assert.Null(incomeResult);
            Assert.Equal(ValidationStatus.Invalid_Amount, validationStatus);
        }

        [Fact]
        public async Task AddIncome_Valid_Input_Returns_Income_And_ValidationStatus_Success()
        {
            var userIdThatExist = 1;
            var income = new Income("incomeTest", 20, new DateTime());

            var (incomeResult, validationStatus) = await _sut.AddIncome(income, userIdThatExist);

            Assert.NotNull(incomeResult);
            Assert.Equal(income.Title, incomeResult.Title);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
        #region GetIncome-Tests
        [Fact]
        public async Task GetIncome_Income_NotFound_Returns_Null()
        {
            int incomeId = 20;

            var incomeResult = await _sut.GetIncome(incomeId);

            Assert.Null(incomeResult);
        }

        [Fact]
        public async Task GetIncome_Income_Found_Returns_Income()
        {
            int incomeId = 1;
            _mockDataContext.Setup(x => x.Incomes.FindAsync(incomeId))
                            .ReturnsAsync(_incomeList.FirstOrDefault(i => i.Id == incomeId));

            var incomeResult = await _sut.GetIncome(incomeId);

            Assert.NotNull(incomeResult);
            Assert.Equal("TestTitle1", incomeResult.Title);
            _mockDataContext.Verify(x => x.Incomes, Times.Once);
        }
        #endregion
        #region GetUserIncomes-Tests
        [Fact]
        public async Task GetUserIncomes_No_Incomes_Returns_Empty_List()
        {
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword($"Password20!", salt);
            var user = new User($"TestUser20", salt, hash);
            user.Id = 20;           

            var incomeResult = await _sut.GetUserIncomes(user.Id);

            Assert.NotNull(incomeResult);
            Assert.Empty(incomeResult);
        }

        [Fact]
        public async Task GetUserIncomes_With_One_Income_Returns_List_With_One_Income()
        {
            var userId = 1;

            var incomeResult = await _sut.GetUserIncomes(userId);

            Assert.NotNull(incomeResult);
            Assert.Single(incomeResult);
            var firstIncome = incomeResult.First();
            Assert.Equal("TestTitle1", firstIncome.Title);
        }
        #endregion
        #region EditIncome-Tests
        [Fact]
        public async Task EditIncome_Income_NotFound_Returns_Null_And_ValidationStatus_NotFound()
        {
            int incomeId = 20; 
            var editedIncome = new Income("UpdatedTitle", 30, new DateTime());

            var (incomeResult, validationStatus) = await _sut.EditIncome(editedIncome, incomeId);

            Assert.Null(incomeResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("posafvhopsuvhaopsfhvsopafuvhasfuopvhasfpuiovhapfuiovhasuiopfvhapsdfuiovhafpsduiovh")]
        public async Task EditIncome_Income_Title_Wrong_Size_Returns_Null_And_ValidationStatus_Invalid_Amount_Of_Chars(string updatedIncomeTitle)
        {
            int incomeId = 1;
            var editedIncome = new Income(updatedIncomeTitle, 30, new DateTime());

            _mockDataContext.Setup(x => x.Incomes.FindAsync(incomeId))
                            .ReturnsAsync(_incomeList.FirstOrDefault(i => i.Id == incomeId));

            var (incomeResult, validationStatus) = await _sut.EditIncome(editedIncome, incomeId);

            Assert.Null(incomeResult);
            Assert.Equal(ValidationStatus.Invalid_Amount_Of_Characters, validationStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(99999999999999)]
        public async Task EditIncome_Income_Amount_Is_Invalid_Returns_Null_And_ValidationStatus_Invalid_Amount(double updatedIncomeAmount)
        {
            int incomeId = 1; 
            var editedIncome = new Income("UpdatedTitle", updatedIncomeAmount, new DateTime());

            _mockDataContext.Setup(x => x.Incomes.FindAsync(incomeId))
                            .ReturnsAsync(_incomeList.FirstOrDefault(i => i.Id == incomeId));

            var (incomeResult, validationStatus) = await _sut.EditIncome(editedIncome, incomeId);

            Assert.Null(incomeResult);
            Assert.Equal(ValidationStatus.Invalid_Amount, validationStatus);
        }

        [Fact]
        public async Task EditIncome_Valid_Input_Returns_Updated_Income_And_ValidationStatus_Success()
        {
            int incomeId = 1;
            var editedIncome = new Income("UpdatedTitle", 30, new DateTime());

            _mockDataContext.Setup(x => x.Incomes.FindAsync(incomeId))
                            .ReturnsAsync(_incomeList.FirstOrDefault(i => i.Id == incomeId));

            var (incomeResult, validationStatus) = await _sut.EditIncome(editedIncome, incomeId);

            Assert.NotNull(incomeResult);
            Assert.Equal(editedIncome.Title, incomeResult.Title);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
        #region DeleteIncome-Tests
        [Fact]
        public async Task DeleteIncome_Income_NotFound_Returns_Null()
        {
            int incomeId = 20;

            var incomeResult = await _sut.DeleteIncome(incomeId);

            Assert.Null(incomeResult);
        }

        [Fact]
        public async Task DeleteIncome_Income_Found_Returns_Income()
        {
            int incomeId = 1;
            _mockDataContext.Setup(x => x.Incomes.FindAsync(incomeId))
                            .ReturnsAsync(_incomeList.FirstOrDefault(i => i.Id == incomeId));

            _mockDataContext.Setup(x => x.Incomes.Remove(It.IsAny<Income>()))
                            .Callback<Income>(incomeToRemove => _incomeList.Remove(incomeToRemove));

            var incomeResult = await _sut.DeleteIncome(incomeId);

            Assert.NotNull(incomeResult);
            Assert.Equal(incomeId, incomeResult.Id);
            Assert.DoesNotContain(incomeResult, _incomeList);
            _mockDataContext.Verify(x => x.Incomes.Remove(It.IsAny<Income>()), Times.Once);
            _mockDataContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
        #endregion
        #region GetUserIncomesByMonth-Tests
        [Fact]
        public async Task GetUserIncomesByMonth_No_Incomes_Returns_Empty_List()
        {
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword($"Password20!", salt);
            var user = new User($"TestUser20", salt, hash);
            user.Id = 20;

            var incomeResult = await _sut.GetUserIncomesByMonth(user.Id, 2022, 3);

            Assert.NotNull(incomeResult);
            Assert.Empty(incomeResult);
        }

        [Fact]
        public async Task GetUserIncomesByMonth_With_One_Income_Returns_List_With_One_Income()
        {
            var userId = 1;

            var incomeResult = await _sut.GetUserIncomesByMonth(userId, 2023, 1);

            Assert.NotNull(incomeResult);
            Assert.Single(incomeResult);
            var firstIncome = incomeResult.First();
            Assert.Equal(2023, firstIncome.Date.Year);
            Assert.Equal(1, firstIncome.Date.Month);
        }
        #endregion
    }
}
