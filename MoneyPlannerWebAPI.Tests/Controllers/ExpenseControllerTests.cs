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
        public async Task CreateExpense_Exception_Returns_500()
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
        [Fact]
        public async Task GetExpense_Expense_NotFound_Returns_404()
        {
            _expenseRepositoryMock.Setup(x => x.GetExpense(1))
                               .ReturnsAsync(null as Expense);

            var result = await _sut.GetExpense(1);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("Expense with Id: 1, could not be found.", objectResult.Value);
        }

        [Fact]
        public async Task GetExpense_Expense_Found_Returns_200()
        {
            var expense = new Expense("testIncome", 500, new DateTime());
            expense.Id = 1;
            expense.ReOccuring = false;

            _expenseRepositoryMock.Setup(x => x.GetExpense(1))
                                  .ReturnsAsync(expense);

            _mockMapper.Setup(x => x.Map<GetExpenseDto>(expense))
                       .Returns(new GetExpenseDto());

            var result = await _sut.GetExpense(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetExpenseDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetExpense_Exception_Returns_500()
        {
            _expenseRepositoryMock.Setup(repo => repo.GetExpense(1))
                                  .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.GetExpense(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUserExpenses-Tests
        [Fact]
        public async Task GetUserExpenses_Returns_List_Of_Expenses_Returns_200()
        {
            int userId = 1;
            var mockExpenseList = new List<Expense>
            {
                new Expense("income1", 200, new DateTime()) { Id = 1, ReOccuring = false },
                new Expense("income2", 200, new DateTime()) { Id = 2, ReOccuring = false },
            };

            _expenseRepositoryMock.Setup(repo => repo.GetUserExpenses(userId))
                                  .ReturnsAsync(mockExpenseList);

            _mockMapper.Setup(x => x.Map<List<GetExpenseDto>>(It.IsAny<List<Expense>>()))
                       .Returns(new List<GetExpenseDto>());

            var result = await _sut.GetUserExpenses(userId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<List<GetExpenseDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetUserExpenses_InternalServerError_Returns_500()
        {
            int userId = 1;

            _expenseRepositoryMock.Setup(repo => repo.GetUserExpenses(userId))
                                  .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.GetUserExpenses(userId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region EditExpense-Tests
        [Fact]
        public async Task EditExpense_Valid_Input_Returns_200()
        {
            int expenseId = 1;

            var postExpenseDto = new PostExpenseDto("testTitle", 20, new DateTime());
            var editedExpense = new Expense("testTitle", 20, new DateTime());
            editedExpense.Id = expenseId;
            editedExpense.ReOccuring = false;

            _expenseRepositoryMock.Setup(repo => repo.EditExpense(It.IsAny<Expense>(), expenseId))
                                 .ReturnsAsync((editedExpense, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<GetExpenseDto>(editedExpense))
                       .Returns(new GetExpenseDto());

            var result = await _sut.EditExpense(postExpenseDto, expenseId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<GetExpenseDto>(okResult.Value);
        }

        [Fact]
        public async Task EditExpense_ExpenseNotFound_Returns_404()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.EditExpense(It.IsAny<Expense>(), expenseId))
                                 .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.EditExpense(new PostExpenseDto("testTitle", 20, new DateTime()), expenseId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Expense not found", notFoundResult.Value);
        }

        [Fact]
        public async Task EditExpense_InvalidAmountOfCharacters_Returns_400()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.EditExpense(It.IsAny<Expense>(), expenseId))
                                  .ReturnsAsync((null, ValidationStatus.Invalid_Amount_Of_Characters));

            var result = await _sut.EditExpense(new PostExpenseDto("testTitle", 20, new DateTime()), expenseId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid_Amount_Of_Characters in the title.", badRequestResult.Value);
        }

        [Fact]
        public async Task EditExpense_InvalidAmount_Returns_400()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.EditExpense(It.IsAny<Expense>(), expenseId))
                                  .ReturnsAsync((null, ValidationStatus.Invalid_Amount));

            var result = await _sut.EditExpense(new PostExpenseDto("testTitle", 20, new DateTime()), expenseId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid_Amount", badRequestResult.Value);
        }

        [Fact]
        public async Task EditExpense_InternalServerError_Returns_500()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.EditExpense(It.IsAny<Expense>(), expenseId))
                                 .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.EditExpense(new PostExpenseDto("testTitle", 20, new DateTime()), expenseId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region DeleteExpense-Tests
        [Fact]
        public async Task DeleteExpense_ValidId_Returns_204()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.DeleteExpense(expenseId))
                                 .ReturnsAsync(new Expense("asd", 20, new DateTime()) { Id = expenseId });
            var result = await _sut.DeleteExpense(expenseId);

            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteExpense_Expense_NotFound_Returns_404()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.DeleteExpense(expenseId))
                                  .ReturnsAsync(null as Expense);

            var result = await _sut.DeleteExpense(expenseId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal($"Expense with Id: {expenseId}, could not be found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteExpense_InternalServerError_Returns_500()
        {
            int expenseId = 1;

            _expenseRepositoryMock.Setup(repo => repo.DeleteExpense(expenseId))
                                  .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.DeleteExpense(expenseId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUserExpensesByMonth-Tests
        [Fact]
        public async Task GetUserExpensesByMonth_Returns_List_Of_Expenses_Returns_200()
        {
            int userId = 1;
            var dateTime = new DateTime(2023, 12, 1);

            var mockExpenseList = new List<Expense>
            {
                new Expense("expense1", 200, dateTime) { Id = 1, ReOccuring = false },
                new Expense("expense2", 200, dateTime) { Id = 2, ReOccuring = false },
            };

            _expenseRepositoryMock.Setup(repo => repo.GetUserExpensesByMonth(userId, 2023, 12))
                                  .ReturnsAsync(mockExpenseList);

            _mockMapper.Setup(x => x.Map<List<GetExpenseDto>>(It.IsAny<List<Expense>>()))
                       .Returns(new List<GetExpenseDto>());

            var result = await _sut.GetUserExpensesByMonth(userId, 2023, 12);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<List<GetExpenseDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetUserExpensesByMonth_InternalServerError_Returns_500()
        {
            int userId = 1;

            _expenseRepositoryMock.Setup(repo => repo.GetUserExpensesByMonth(userId, 2022, 3))
                                  .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.GetUserExpensesByMonth(userId, 2022, 3);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
    }
}