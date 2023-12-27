using Entity;
using Infrastructure.Repositories.AnalysisRepo;
using Infrastructure.Utilities;
using Moq;
using Moq.EntityFrameworkCore;
using System.Text;

namespace Infrastructure.Tests.Repositories.AnalysisRepo
{
    public class AnalysisRepositoryTests
    {
        private Mock<DataContext> _mockDataContext;
        private List<User> _userList;
        private List<Income> _incomeList;
        private List<Expense> _expenseList;
        private List<MonthAnalysis> _monthAnalysisList;
        private AnalysisRepository _sut;
        public AnalysisRepositoryTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _userList = SeedUsers();
            _incomeList = SeedIncomes();
            _expenseList = SeedExpenses();
            _monthAnalysisList = SeedMonthAnalysis();
            _mockDataContext.Setup(x => x.Users)
                            .ReturnsDbSet(_userList);
            _mockDataContext.Setup(x => x.Incomes)
                            .ReturnsDbSet(_incomeList);
            _mockDataContext.Setup(x => x.Expenses)
                            .ReturnsDbSet(_expenseList);
            _mockDataContext.Setup(x => x.MonthAnalysis)
                            .ReturnsDbSet(_monthAnalysisList);
            _sut = new AnalysisRepository(_mockDataContext.Object);
        }
        #region Private Seeding
        private static List<User> SeedUsers()
        {
            var userList = new List<User>();

            for (int i = 1; i < 7; i++)
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
        private static List<MonthAnalysis> SeedMonthAnalysis()
        {
            var users = SeedUsers();
            var incomes = SeedIncomes();
            var expenses = SeedExpenses();
            var monthAnalysisList = new List<MonthAnalysis>();

            for (int i = 1; i < 6; i++)
            {
                monthAnalysisList.Add(new MonthAnalysis
                { Id = i, User = users[i - 1], Expenses = expenses, Incomes = incomes, Month = i, Year = 2023, SummaryAmount = 100 });
            }
            return monthAnalysisList;
        }
        #endregion
        #region CreateMonthAnalysis-Tests
        [Fact]
        public async Task CreateMonthAnalysis_User_NotFound_Returns_Null_And_ValidationStatus_Not_Found()
        {
            var userIdThatDoesntExist = 10;            

            var (monthAnalysisResult, validationStatus) = await _sut.CreateMonthAnalysis(1, 2023, userIdThatDoesntExist);

            Assert.Null(monthAnalysisResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Fact]
        public async Task CreateMonthAnalysis_No_Incomes_Or_Expense_Returns_Null_And_ValidationStatus_No_Data()
        {
            var userId = 6;

            _mockDataContext.Setup(x => x.MonthAnalysis)
                            .ReturnsDbSet(new List<MonthAnalysis>());

            _mockDataContext.Setup(x => x.Incomes)
                            .ReturnsDbSet(new List<Income>());
            _mockDataContext.Setup(x => x.Expenses)
                            .ReturnsDbSet(new List<Expense>());

            var (monthAnalysisResult, validationStatus) = await _sut.CreateMonthAnalysis(1, 2023, userId);

            Assert.Null(monthAnalysisResult);
            Assert.Equal(ValidationStatus.No_Data_To_Make_Analysis, validationStatus);
        }

        [Fact]
        public async Task CreateMonthAnalysis_With_Incomes_Or_Expense_Returns_Analysis_And_ValidationStatus_Success()
        {
            var userId = 1;

            _mockDataContext.Setup(x => x.MonthAnalysis)
                            .ReturnsDbSet(new List<MonthAnalysis>());

            var incomes = _incomeList.Where(i => i.User.Id == userId && i.Date.Year == 2023 && i.Date.Month == 1).ToList();
            var expenses = _expenseList.Where(e => e.User.Id == userId && e.Date.Year == 2023 && e.Date.Month == 1).ToList();

            _mockDataContext.Setup(x => x.Incomes)
                            .ReturnsDbSet(incomes);

            _mockDataContext.Setup(x => x.Expenses)
                            .ReturnsDbSet(expenses);

            var (monthAnalysisResult, validationStatus) = await _sut.CreateMonthAnalysis(1, 2023, userId);

            Assert.NotNull(monthAnalysisResult);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }

        [Fact]
        public async Task CreateMonthAnalysis_With_Deletes_Previous_MonthAnalysis_Returns_Analysis_And_ValidationStatus_Success()
        {
            var userId = 1;

            var monthAnalyses = new List<MonthAnalysis>()
            {
                new MonthAnalysis{ Id = 45, Year = 2023, Month = 1, Expenses = new List<Expense>(), Incomes = new List<Income>(), SummaryAmount = 100, User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 } },
            };

            _mockDataContext.Setup(x => x.MonthAnalysis)
                            .ReturnsDbSet(monthAnalyses);

            var incomes = _incomeList.Where(i => i.User.Id == userId && i.Date.Year == 2023 && i.Date.Month == 1).ToList();
            var expenses = _expenseList.Where(e => e.User.Id == userId && e.Date.Year == 2023 && e.Date.Month == 1).ToList();

            _mockDataContext.Setup(x => x.Incomes)
                            .ReturnsDbSet(incomes);

            _mockDataContext.Setup(x => x.Expenses)
                            .ReturnsDbSet(expenses);

            var (monthAnalysisResult, validationStatus) = await _sut.CreateMonthAnalysis(1, 2023, userId);

            Assert.NotNull(monthAnalysisResult);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
        #region GetMonthAnalysis-Tests
        [Fact]
        public async Task GetMonthAnalysis_MonthAnalysis_NotFound_Returns_Null()
        {
            int monthAnalysisId = 20;

            var monthResult = await _sut.GetMonthAnalysis(monthAnalysisId);

            Assert.Null(monthResult);
        }

        [Fact]
        public async Task GetMonthAnalysis_MonthAnalysis_Found_Returns_MonthAnalysis()
        {
            int monthAnalysisId = 1;

            _mockDataContext.Setup(x => x.MonthAnalysis.FindAsync(monthAnalysisId))
                            .ReturnsAsync(_monthAnalysisList.FirstOrDefault(i => i.Id == monthAnalysisId));

            var monthResult = await _sut.GetMonthAnalysis(monthAnalysisId);

            Assert.NotNull(monthResult);
            Assert.Equal(1, monthResult.Month);
            _mockDataContext.Verify(x => x.MonthAnalysis, Times.Once);
        }
        #endregion
        #region GetMonthAnalysisByMonth-Tests
        [Fact]
        public async Task GetMonthAnalysisByMonth_No_User_Returns_Null()
        {
            int userId = 20;

            var monthResult = await _sut.GetMonthAnalysisByMonth(5, 2022, userId);

            Assert.Null(monthResult);
        }

        [Fact]
        public async Task GetMonthAnalysisByMonth_Analysis_Exists_Returns_Correct_Analysis()
        {
            int userId = 1;

            var monthResult = await _sut.GetMonthAnalysisByMonth(1, 2023, userId);

            Assert.NotNull(monthResult);
            Assert.NotEmpty(monthResult.Expenses!);
            Assert.NotEmpty(monthResult.Incomes!);
        }

        [Fact]
        public async Task GetMonthAnalysisByMonth_Analysis_Not_Exists_Returns_Null()
        {
            int userId = 6;

            var monthResult = await _sut.GetMonthAnalysisByMonth(1, 2023, userId);

            Assert.Null(monthResult);            
        }
        #endregion
    }
}
