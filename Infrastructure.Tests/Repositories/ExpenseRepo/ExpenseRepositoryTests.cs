using Entity;
using Infrastructure.Repositories.ExpenseRepo;
using Infrastructure.Utilities;
using Moq;
using Moq.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories.ExpenseRepo
{
    public class ExpenseRepositoryTests
    {
        private Mock<DataContext> _mockDataContext;
        private List<User> _userList;
        private List<Expense> _expenseList;
        private ExpenseRepository _sut;
        public ExpenseRepositoryTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _userList = SeedUsers();
            _expenseList = SeedExpenses();
            _mockDataContext.Setup(x => x.Users)
                            .ReturnsDbSet(_userList);
            _mockDataContext.Setup(x => x.Expenses)
                            .ReturnsDbSet(_expenseList);
            _sut = new ExpenseRepository(_mockDataContext.Object);
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
        private static List<Expense> SeedExpenses()
        {
            var users = SeedUsers();
            var expenseList = new List<Expense>();

            for (int i = 1; i < 6; i++)
            {
                expenseList.Add(new Expense($"TestTitle{i}", i, new DateTime(2023, i, 5))
                { Id = i, ReOccuring = false, User = users[i - 1] });
            }
            return expenseList;
        }
        #endregion
        #region AddExpense-Tests
        [Fact]
        public async Task AddExpense_User_NotFound_Returns_Null_And_ValidationStatus_Not_Found()
        {
            var userIdThatDoesntExist = 10;
            var expense = new Expense("incomeTest", 20, new DateTime());

            var (expenseResult, validationStatus) = await _sut.AddExpense(expense, userIdThatDoesntExist);

            Assert.Null(expenseResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("posafvhopsuvhaopsfhvsopafuvhasfuopvhasfpuiovhapfuiovhasuiopfvhapsdfuiovhafpsduiovh")]
        public async Task AddIncome_Income_Title_Wrong_Size_Returns_Null_And_ValidationStatus_Invalid_Amount_Of_Chars(string expenseTitle)
        {
            var userIdThatExist = 1;
            var expense = new Expense(expenseTitle, 20, new DateTime());

            var (expenseResult, validationStatus) = await _sut.AddExpense(expense, userIdThatExist);

            Assert.Null(expenseResult);
            Assert.Equal(ValidationStatus.Invalid_Amount_Of_Characters, validationStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(99999999999999)]
        public async Task AddExpense_Expense_Amount_Is_Invalid_Returns_Null_And_ValidationStatus_Invalid_Amount(double expenseAmount)
        {
            var userIdThatExist = 1;
            var expense = new Expense("testTitle", expenseAmount, new DateTime());

            var (expenseResult, validationStatus) = await _sut.AddExpense(expense, userIdThatExist);

            Assert.Null(expenseResult);
            Assert.Equal(ValidationStatus.Invalid_Amount, validationStatus);
        }

        [Fact]
        public async Task AddExpense_Valid_Input_Returns_Expense_And_ValidationStatus_Success()
        {
            var userIdThatExist = 1;
            var expense = new Expense("incomeTest", 20, new DateTime());

            var (expenseResult, validationStatus) = await _sut.AddExpense(expense, userIdThatExist);

            Assert.NotNull(expenseResult);
            Assert.Equal(expense.Title, expenseResult.Title);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
        #region GetExpense-Tests
        [Fact]
        public async Task GetExpense_Expense_NotFound_Returns_Null()
        {
            int expenseId = 20;

            var expenseResult = await _sut.GetExpense(expenseId);

            Assert.Null(expenseResult);
        }

        [Fact]
        public async Task GetExpense_Expense_Found_Returns_Income()
        {
            int expenseId = 1;
            _mockDataContext.Setup(x => x.Expenses.FindAsync(expenseId))
                            .ReturnsAsync(_expenseList.FirstOrDefault(i => i.Id == expenseId));

            var expenseResult = await _sut.GetExpense(expenseId);

            Assert.NotNull(expenseResult);
            Assert.Equal("TestTitle1", expenseResult.Title);
            _mockDataContext.Verify(x => x.Expenses, Times.Once);
        }
        #endregion
        #region GetUserExpenses-Tests
        [Fact]
        public async Task GetUserExpenses_No_Expenses_Returns_Empty_List()
        {
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword($"Password20!", salt);
            var user = new User($"TestUser20", salt, hash);
            user.Id = 20;

            var expenseResult = await _sut.GetUserExpenses(user.Id);

            Assert.NotNull(expenseResult);
            Assert.Empty(expenseResult);
        }

        [Fact]
        public async Task GetUserExpenses_With_One_Expense_Returns_List_With_One_Expense()
        {
            var userId = 1;

            var expenseResult = await _sut.GetUserExpenses(userId);

            Assert.NotNull(expenseResult);
            Assert.Single(expenseResult);
            var firstExpense = expenseResult.First();
            Assert.Equal("TestTitle1", firstExpense.Title);
        }
        #endregion
        #region EditExpense-Tests
        [Fact]
        public async Task EditExpense_Expense_NotFound_Returns_Null_And_ValidationStatus_NotFound()
        {
            int expenseId = 20;
            var editedExpense = new Expense("UpdatedTitle", 30, new DateTime());

            var (expenseResult, validationStatus) = await _sut.EditExpense(editedExpense, expenseId);

            Assert.Null(expenseResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("posafvhopsuvhaopsfhvsopafuvhasfuopvhasfpuiovhapfuiovhasuiopfvhapsdfuiovhafpsduiovh")]
        public async Task EditExpense_Expense_Title_Wrong_Size_Returns_Null_And_ValidationStatus_Invalid_Amount_Of_Chars(string updatedExpenseTitle)
        {
            int expenseId = 1;
            var editedExpense = new Expense(updatedExpenseTitle, 30, new DateTime());

            _mockDataContext.Setup(x => x.Expenses.FindAsync(expenseId))
                            .ReturnsAsync(_expenseList.FirstOrDefault(i => i.Id == expenseId));

            var (expenseResult, validationStatus) = await _sut.EditExpense(editedExpense, expenseId);

            Assert.Null(expenseResult);
            Assert.Equal(ValidationStatus.Invalid_Amount_Of_Characters, validationStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(99999999999999)]
        public async Task EditExpense_Expense_Amount_Is_Invalid_Returns_Null_And_ValidationStatus_Invalid_Amount(double updatedExpenseAmount)
        {
            int expenseId = 1;
            var editedExpense = new Expense("UpdatedTitle", updatedExpenseAmount, new DateTime());

            _mockDataContext.Setup(x => x.Expenses.FindAsync(expenseId))
                            .ReturnsAsync(_expenseList.FirstOrDefault(i => i.Id == expenseId));

            var (expenseResult, validationStatus) = await _sut.EditExpense(editedExpense, expenseId);

            Assert.Null(expenseResult);
            Assert.Equal(ValidationStatus.Invalid_Amount, validationStatus);
        }

        [Fact]
        public async Task EditExpense_Valid_Input_Returns_Updated_Expense_And_ValidationStatus_Success()
        {
            int expenseId = 1;
            var editedExpense = new Expense("UpdatedTitle", 30, new DateTime());

            _mockDataContext.Setup(x => x.Expenses.FindAsync(expenseId))
                            .ReturnsAsync(_expenseList.FirstOrDefault(i => i.Id == expenseId));

            var (expenseResult, validationStatus) = await _sut.EditExpense(editedExpense, expenseId);

            Assert.NotNull(expenseResult);
            Assert.Equal(editedExpense.Title, expenseResult.Title);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
    }
}
