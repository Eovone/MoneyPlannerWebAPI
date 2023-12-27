using AutoMapper;
using Entity;
using Infrastructure.Repositories.AnalysisRepo;
using Infrastructure.Repositories.IncomeRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.AnalysisDto;
using MoneyPlannerWebAPI.DTO.ExpenseDto;
using MoneyPlannerWebAPI.DTO.IncomeDto;
using MoneyPlannerWebAPI.Utilities;
using Moq;
using System.Security.Claims;
using System.Text;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class AnalysisControllerTests
    {
        private Mock<IAnalysisRepository> _analysisRepositoryMock;
        private Mock<ILogger<AnalysisController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private Mock<IAuthorizationHelper> _authorizationHelperMock;
        private AnalysisController _sut;
        public AnalysisControllerTests()
        {
            _analysisRepositoryMock = new Mock<IAnalysisRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<AnalysisController>>();
            _authorizationHelperMock = new Mock<IAuthorizationHelper>();

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(true);

            _sut = new AnalysisController(_mockMapper.Object, _analysisRepositoryMock.Object, _mockLogger.Object, _authorizationHelperMock.Object);
        }
        #region CreateMonthAnalysis-Tests
        [Fact]
        public async Task CreateMonthAnalysis_UnauthorizedUser_Returns_401()
        {
            var postMonthlyAnalysisDto = new PostMonthlyAnalysisDto { Month = 12, Year = 2023 };
            int unauthorizedUserId = 2;

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(false);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "SomeUsername"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity!.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Claims).Returns(claims);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            var result = await _sut.CreateMonthAnalysis(postMonthlyAnalysisDto, unauthorizedUserId);

            Assert.NotNull(result);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task CreateMonthAnalysis_User_NotFound_Returns_404()
        {
            var postMonthlyAnalysisDto = new PostMonthlyAnalysisDto { Month = 12, Year = 2023 };
            int userId = 1;

            _analysisRepositoryMock.Setup(repo => repo.CreateMonthAnalysis(postMonthlyAnalysisDto.Month, postMonthlyAnalysisDto.Year, userId))
                                   .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.CreateMonthAnalysis(postMonthlyAnalysisDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("User Not Found", objectResult.Value);
        }

        [Fact]
        public async Task CreateMonthAnalysis_No_Income_Or_Expense_Returns_400()
        {
            var postMonthlyAnalysisDto = new PostMonthlyAnalysisDto { Month = 12, Year = 2023 };
            int userId = 1;

            _analysisRepositoryMock.Setup(repo => repo.CreateMonthAnalysis(postMonthlyAnalysisDto.Month, postMonthlyAnalysisDto.Year, userId))
                                   .ReturnsAsync((null, ValidationStatus.No_Data_To_Make_Analysis));

            var result = await _sut.CreateMonthAnalysis(postMonthlyAnalysisDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("No_Data_To_Make_Analysis", objectResult.Value);
        }

        [Fact]
        public async Task CreateMonthAnalysis_Valid_Input_Returns_201()
        {
            int userId = 1;

            var postMonthlyAnalysisDto = new PostMonthlyAnalysisDto { Month = 12, Year = 2023 };

            var getMonthlyAnalysisDto = new GetMonthlyAnalysisDto();
            getMonthlyAnalysisDto.Id = 1;
            getMonthlyAnalysisDto.UserId = userId;
            getMonthlyAnalysisDto.Incomes = new List<GetIncomeDto> { };
            getMonthlyAnalysisDto.Expenses = new List<GetExpenseDto> { };
            getMonthlyAnalysisDto.SummaryAmount = 100;
            getMonthlyAnalysisDto.Month = 12;
            getMonthlyAnalysisDto.Year = 2023;

            var monthAnalysis = new MonthAnalysis();
            monthAnalysis.Id = 1;
            monthAnalysis.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 };
            monthAnalysis.Incomes = new List<Income> { };
            monthAnalysis.Expenses = new List<Expense> { };
            monthAnalysis.SummaryAmount = 100;
            monthAnalysis.Month = 12;
            monthAnalysis.Year = 2023;            

            _analysisRepositoryMock.Setup(repo => repo.CreateMonthAnalysis(12, 2023, userId))
                                   .ReturnsAsync((monthAnalysis, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<GetMonthlyAnalysisDto>(monthAnalysis))
                       .Returns(getMonthlyAnalysisDto);

            var result = await _sut.CreateMonthAnalysis(postMonthlyAnalysisDto, userId);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.NotNull(createdAtResult.Value);
            Assert.IsType<GetMonthlyAnalysisDto>(createdAtResult.Value);
        }

        [Fact]
        public async Task CreateMonthAnalysis_Exception_Returns_500()
        {
            int userId = 1;
            var postMonthlyAnalysisDto = new PostMonthlyAnalysisDto { Month = 12, Year = 2023 };

            _analysisRepositoryMock.Setup(repo => repo.CreateMonthAnalysis(12, 2023, userId))
                                   .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.CreateMonthAnalysis(postMonthlyAnalysisDto, userId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetMonthAnalysis-Tests
        [Fact]
        public async Task GetMonthAnalysis_UnauthorizedUser_Returns_401()
        {
            int unauthorizedUserId = 2;
            var monthAnalysis = new MonthAnalysis();
            monthAnalysis.Id = 1;
            monthAnalysis.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 };
            monthAnalysis.Incomes = new List<Income> { };
            monthAnalysis.Expenses = new List<Expense> { };
            monthAnalysis.SummaryAmount = 100;
            monthAnalysis.Month = 12;
            monthAnalysis.Year = 2023;

            _analysisRepositoryMock.Setup(x => x.GetMonthAnalysis(1))
                                   .ReturnsAsync(monthAnalysis);

            _mockMapper.Setup(x => x.Map<GetMonthlyAnalysisDto>(monthAnalysis))
                       .Returns(new GetMonthlyAnalysisDto());


            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(false);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, unauthorizedUserId.ToString()),
                new Claim(ClaimTypes.Name, "SomeUsername"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity!.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Claims).Returns(claims);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            var result = await _sut.GetMonthAnalysis(1);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetMonthAnalysis_MonthAnalysis_NotFound_Returns_404()
        {
            _analysisRepositoryMock.Setup(x => x.GetMonthAnalysis(1))
                                   .ReturnsAsync(null as MonthAnalysis);

            var result = await _sut.GetMonthAnalysis(1);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("MonthAnalysis with Id: 1, could not be found.", objectResult.Value);
        }

        [Fact]
        public async Task GetMonthAnalysis_MonthAnalysis_Found_Returns_200()
        {
            var monthAnalysis = new MonthAnalysis();
            monthAnalysis.Id = 1;
            monthAnalysis.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 };
            monthAnalysis.Incomes = new List<Income> { };
            monthAnalysis.Expenses = new List<Expense> { };
            monthAnalysis.SummaryAmount = 100;
            monthAnalysis.Month = 12;
            monthAnalysis.Year = 2023;

            _analysisRepositoryMock.Setup(x => x.GetMonthAnalysis(1))
                                   .ReturnsAsync(monthAnalysis);

            _mockMapper.Setup(x => x.Map<GetMonthlyAnalysisDto>(monthAnalysis))
                       .Returns(new GetMonthlyAnalysisDto());

            var result = await _sut.GetMonthAnalysis(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetMonthlyAnalysisDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetMonthAnalysis_Exception_Returns_500()
        {
            _analysisRepositoryMock.Setup(repo => repo.GetMonthAnalysis(1))
                                   .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.GetMonthAnalysis(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetMonthAnalysisByMonth-Tests
        [Fact]
        public async Task GetMonthAnalysisByMonth_UnauthorizedUser_Returns_401()
        {
            int unauthorizedUserId = 2;
            var monthAnalysis = new MonthAnalysis();
            monthAnalysis.Id = 1;
            monthAnalysis.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 };
            monthAnalysis.Incomes = new List<Income> { };
            monthAnalysis.Expenses = new List<Expense> { };
            monthAnalysis.SummaryAmount = 100;
            monthAnalysis.Month = 12;
            monthAnalysis.Year = 2023;

            _analysisRepositoryMock.Setup(x => x.GetMonthAnalysisByMonth(12, 2023, unauthorizedUserId))
                                   .ReturnsAsync(monthAnalysis);

            _mockMapper.Setup(x => x.Map<GetMonthlyAnalysisDto>(monthAnalysis))
                       .Returns(new GetMonthlyAnalysisDto());


            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(false);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, unauthorizedUserId.ToString()),
                new Claim(ClaimTypes.Name, "SomeUsername"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity!.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Claims).Returns(claims);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            var result = await _sut.GetMonthAnalysisByMonth(1, 2023, 12);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetMonthAnalysisByMonth_MonthAnalysis_NotFound_Returns_404()
        {
            _analysisRepositoryMock.Setup(x => x.GetMonthAnalysisByMonth(12, 2023, 1))
                                   .ReturnsAsync(null as MonthAnalysis);

            var result = await _sut.GetMonthAnalysisByMonth(1, 2023, 12);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("MonthAnalysis for month: 12, does not exist.", objectResult.Value);
        }

        [Fact]
        public async Task GetMonthAnalysisByMonth_MonthAnalysis_Found_Returns_200()
        {
            var monthAnalysis = new MonthAnalysis();
            monthAnalysis.Id = 1;
            monthAnalysis.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 };
            monthAnalysis.Incomes = new List<Income> { };
            monthAnalysis.Expenses = new List<Expense> { };
            monthAnalysis.SummaryAmount = 100;
            monthAnalysis.Month = 12;
            monthAnalysis.Year = 2023;

            _analysisRepositoryMock.Setup(x => x.GetMonthAnalysisByMonth(12, 2023, 1))
                                   .ReturnsAsync(monthAnalysis);

            _mockMapper.Setup(x => x.Map<GetMonthlyAnalysisDto>(monthAnalysis))
                       .Returns(new GetMonthlyAnalysisDto());

            var result = await _sut.GetMonthAnalysisByMonth(1, 2023, 12);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetMonthlyAnalysisDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetMonthAnalysisByMonth_Exception_Returns_500()
        {
            _analysisRepositoryMock.Setup(repo => repo.GetMonthAnalysisByMonth(12, 2023, 1))
                                   .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.GetMonthAnalysisByMonth(1, 2023, 12);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
    }
}
