using Entity;
using Infrastructure.Repositories.BudgetPlanningRepo;
using Infrastructure.Utilities;
using Moq;
using Moq.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories.BudgetPlanningRepo
{
    public class BudgetPlanningRepositoryTests
    {
        private Mock<DataContext> _mockDataContext;
        private List<BudgetPlan> _budgetPlans;
        private List<BudgetPlanItem> _budgetPlanItems;
        private List<User> _userList;
        private BudgetPlanningRepository _sut;
        public BudgetPlanningRepositoryTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _userList = SeedUsers();
            _budgetPlans = SeedBudgetPlan();
            _budgetPlanItems = SeedBudgetPlanItems();
            _mockDataContext.Setup(x => x.Users)
                            .ReturnsDbSet(_userList);
            _mockDataContext.Setup(x => x.BudgetPlans)
                            .ReturnsDbSet(_budgetPlans);
            _mockDataContext.Setup(x => x.BudgetPlansItems)
                            .ReturnsDbSet(_budgetPlanItems);
            _sut = new BudgetPlanningRepository(_mockDataContext.Object);
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
        private static List<BudgetPlan> SeedBudgetPlan()
        {
            var budgetPlanList = new List<BudgetPlan>();

            var budgetPlan = new BudgetPlan();
            budgetPlan.Id = 1;
            budgetPlan.BudgetPlanItems = SeedBudgetPlanItems();
            budgetPlan.SummaryAmount = 15;
            budgetPlan.UserId = 1;

            budgetPlanList.Add(budgetPlan);

            return budgetPlanList;            
        }
        private static List<BudgetPlanItem> SeedBudgetPlanItems()
        {
            var budgetPlanItemsList = new List<BudgetPlanItem>();

            for (int i = 1; i < 6; i++)
            {
               budgetPlanItemsList.Add(new BudgetPlanItem { Id = i, Amount = i, IsIncome = true, Title = $"TestBudgetPlanItem{i}" });
            }
            return budgetPlanItemsList;
        }
        #endregion
        #region CreateBudgetPlan-Tests
        [Fact]
        public async Task CreateBudgetPlan_User_NotFound_Returns_Null_And_ValidationStatus_Not_Found()
        {
            var userIdThatDoesntExist = 10;
            var budgetPlan = new BudgetPlan();
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem>();          

            var (budgetResult, validationStatus) = await _sut.CreateBudgetPlan(budgetPlan, budgetPlan.BudgetPlanItems, userIdThatDoesntExist);

            Assert.Null(budgetResult);
            Assert.Equal(ValidationStatus.Not_Found, validationStatus);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("posafvhopsuvhaopsfhvsopafuvhasfuopvhasfpuiovhapfuiovhasuiopfvhapsdfuiovhafpsduiovh")]
        public async Task CreateBudgetPlan_Items_Title_Wrong_Size_Returns_Null_And_ValidationStatus_Invalid_Amount_Of_Chars(string itemTitle)
        {
            var userIdThatExist = 2;
            var budgetPlan = new BudgetPlan();
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem> 
                { new BudgetPlanItem { Title = itemTitle } };


            var (budgetResult, validationStatus) = await _sut.CreateBudgetPlan(budgetPlan, budgetPlan.BudgetPlanItems, userIdThatExist);

            Assert.Null(budgetResult);
            Assert.Equal(ValidationStatus.Invalid_Amount_Of_Characters, validationStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(99999999999999)]
        public async Task CreateBudgetPlan_Item_Amount_Is_Invalid_Returns_Null_And_ValidationStatus_Invalid_Amount(double itemAmount)
        {
            var userIdThatExist = 2;
            var budgetPlan = new BudgetPlan();
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem>
                { new BudgetPlanItem { Amount = itemAmount, Title = "TestTitle"} };


            var (budgetResult, validationStatus) = await _sut.CreateBudgetPlan(budgetPlan, budgetPlan.BudgetPlanItems, userIdThatExist);

            Assert.Null(budgetResult);
            Assert.Equal(ValidationStatus.Invalid_Amount, validationStatus);
        }

        [Fact]
        public async Task CreateBudgetPlan_Valid_Input_Returns_BudgetPlan_And_ValidationStatus_Success()
        {
            var userIdThatExist = 2;
            var budgetPlan = new BudgetPlan();         
            budgetPlan.SummaryAmount = 15;

            var budgetPlanItems = new List<BudgetPlanItem>
                { new BudgetPlanItem { Amount = 15, Title = "TestTitle", IsIncome = true } };

            var (budgetResult, validationStatus) = await _sut.CreateBudgetPlan(budgetPlan, budgetPlanItems, userIdThatExist);

            Assert.NotNull(budgetResult);
            Assert.Equal(userIdThatExist, budgetResult.UserId);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }

        [Fact]
        public async Task CreateBudgetPlan_Valid_Input_But_Removes_Previous_BudgetPlan_Returns_BudgetPlan_And_ValidationStatus_Success()
        {
            var userIdThatHasBudgetPlan = 1;
            var budgetPlan = new BudgetPlan();
            budgetPlan.SummaryAmount = 15;

            var budgetPlanItems = new List<BudgetPlanItem>
                { new BudgetPlanItem { Amount = 15, Title = "TestTitle", IsIncome = true } };

            var (budgetResult, validationStatus) = await _sut.CreateBudgetPlan(budgetPlan, budgetPlanItems, userIdThatHasBudgetPlan);

            Assert.NotNull(budgetResult);
            Assert.Equal(userIdThatHasBudgetPlan, budgetResult.UserId);
            Assert.Equal(ValidationStatus.Success, validationStatus);
        }
        #endregion
        #region GetBudgetPlan-Tests
        [Fact]
        public async Task GetBudgetPlan_BudgetPlan_NotFound_Returns_Null()
        {
            int budgetPlanId = 20;

            var budgetResult = await _sut.GetBudgetPlan(budgetPlanId);

            Assert.Null(budgetResult);
        }

        [Fact]
        public async Task GetBudgetPlan_BudgetPlan_Found_Returns_Income()
        {
            int budgetPlanId = 1;

            _mockDataContext.Setup(x => x.BudgetPlans.FindAsync(budgetPlanId))
                            .ReturnsAsync(_budgetPlans.FirstOrDefault(bp => bp.Id == budgetPlanId));

            var budgetResult = await _sut.GetBudgetPlan(budgetPlanId);

            Assert.NotNull(budgetResult);
            Assert.Equal(15, budgetResult.SummaryAmount);
            _mockDataContext.Verify(x => x.BudgetPlans, Times.Once);
        }
        #endregion
        #region GetUserBudgetPlan-Tests
        [Fact]
        public async Task GetUserBudgetPlan_No_BudgetPlan_Returns_Null()
        {
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword($"Password20!", salt);
            var user = new User($"TestUser20", salt, hash);
            user.Id = 20;

            var budgetResult = await _sut.GetUserBudgetPlan(user.Id);

            Assert.Null(budgetResult);
        }

        [Fact]
        public async Task GetUserBudgetPlan_With_BudgetPlan_Returns_BudgetPlan()
        {
            int userId = 1;

            var budgetResult = await _sut.GetUserBudgetPlan(userId);

            Assert.NotNull(budgetResult);
            Assert.Equal(userId, budgetResult.UserId);
            Assert.Equal(15, budgetResult.SummaryAmount);
        }
        #endregion
    }
}