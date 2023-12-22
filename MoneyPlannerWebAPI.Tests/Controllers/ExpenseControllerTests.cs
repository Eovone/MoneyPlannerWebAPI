using AutoMapper;
using Entity;
using Infrastructure.Repositories.ExpenseRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.ExpenseDto;
using MoneyPlannerWebAPI.DTO.IncomeDto;
using Moq;
using System.Text;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class ExpenseControllerTests
    {
        private Mock<IExpenseRepository> _expenseRepositoryMock;
        private Mock<ILogger<ExpenseController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private ExpenseController _sut;
        public ExpenseControllerTests()
        {
            _expenseRepositoryMock = new Mock<IExpenseRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ExpenseController>>();

            _sut = new ExpenseController(_mockMapper.Object, _expenseRepositoryMock.Object, _mockLogger.Object);
        }
        #region CreateExpense-Tests
        [Fact]
        public async Task CreateExpense_User_NotFound_Returns_404()
        {
            var postExpenseDto = new PostExpenseDto("testTitle", 20, new DateTime());
            postExpenseDto.ReOccuring = false;
            int userId = 1;

            _expenseRepositoryMock.Setup(repo => repo.AddExpense(It.IsAny<Expense>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.CreateExpense(postExpenseDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("User Not Found", objectResult.Value);
        }
        [Fact]
        public async Task CreateExpense_InvalidAmount_Of_Chars_In_Title_Returns_400()
        {
            var postExpenseDto = new PostExpenseDto("a", 20, new DateTime());
            int userId = 1;

            _expenseRepositoryMock.Setup(repo => repo.AddExpense(It.IsAny<Expense>(), userId))
                                  .ReturnsAsync((null, ValidationStatus.Invalid_Amount_Of_Characters));

            var result = await _sut.CreateExpense(postExpenseDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Amount_Of_Characters in the title.", objectResult.Value);
        }

        [Fact]
        public async Task CreateExpense_Invalid_Amount_Returns_400()
        {
            var postExpenseDto = new PostExpenseDto("testTitle", 0, new DateTime());
            int userId = 1;

            _expenseRepositoryMock.Setup(repo => repo.AddExpense(It.IsAny<Expense>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount));

            var result = await _sut.CreateExpense(postExpenseDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Amount", objectResult.Value);
        }

        [Fact]
        public async Task CreateExpense_Valid_Input_Returns_201()
        {
            int userId = 1;

            var postExpenseDto = new PostExpenseDto("testTitle", 20, new DateTime());

            var getExpenseDto = new GetExpenseDto();
            getExpenseDto.Id = 1;
            getExpenseDto.Date = new DateTime();
            getExpenseDto.Title = "testTitle";
            getExpenseDto.Amount = 20;
            getExpenseDto.ReOccuring = false;

            var expense = new Expense("testTitle", 20, new DateTime());
            expense.Id = 1;
            expense.ReOccuring = false;
            expense.MonthAnalysis = new List<MonthAnalysis>();
            expense.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");

            _expenseRepositoryMock.Setup(repo => repo.AddExpense(It.IsAny<Expense>(), userId))
                                 .ReturnsAsync((expense, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<GetExpenseDto>(expense))
                       .Returns(getExpenseDto);

            var result = await _sut.CreateExpense(postExpenseDto, userId);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.NotNull(createdAtResult.Value);
            Assert.IsType<GetExpenseDto>(createdAtResult.Value);
        }

        [Fact]
        public async Task CreateIncome_Exception_Returns_500()
        {
            int userId = 1;
            var postExpenseDto = new PostExpenseDto("testTitle", 20, new DateTime());

            _expenseRepositoryMock.Setup(repo => repo.AddExpense(It.IsAny<Expense>(), userId))
                                  .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.CreateExpense(postExpenseDto, userId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetExpense-Tests

        #endregion
    }

}
