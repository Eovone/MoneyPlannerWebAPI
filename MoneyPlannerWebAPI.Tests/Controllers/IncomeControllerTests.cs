using AutoMapper;
using Entity;
using Infrastructure.Repositories.IncomeRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.IncomeDto;
using MoneyPlannerWebAPI.Utilities;
using Moq;
using System.Security.Claims;
using System.Text;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class IncomeControllerTests
    {
        private Mock<IIncomeRepository> _incomeRepositoryMock;
        private Mock<ILogger<IncomeController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private Mock<IAuthorizationHelper> _authorizationHelperMock;
        private IncomeController _sut;
        public IncomeControllerTests()
        {
            _incomeRepositoryMock = new Mock<IIncomeRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<IncomeController>>();
            _authorizationHelperMock = new Mock<IAuthorizationHelper>();

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(true);

            _sut = new IncomeController(_mockMapper.Object, _incomeRepositoryMock.Object, _mockLogger.Object, _authorizationHelperMock.Object);
        }
        #region CreateIncome-Tests
        [Fact]
        public async Task CreateIncome_UnauthorizedUser_Returns_401()
        {
            var postIncomeDto = new PostIncomeDto("testTitle", 20, new DateTime());
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

            var result = await _sut.CreateIncome(postIncomeDto, unauthorizedUserId);

            Assert.NotNull(result);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task CreateIncome_User_NotFound_Returns_404()
        {
            var postIncomeDto = new PostIncomeDto("testTitle", 20, new DateTime());
            postIncomeDto.ReOccuring = false;
            int userId = 1;

            _incomeRepositoryMock.Setup(repo => repo.AddIncome(It.IsAny<Income>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.CreateIncome(postIncomeDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("User Not Found", objectResult.Value);
        }

        [Fact]
        public async Task CreateIncome_InvalidAmount_Of_Chars_In_Title_Returns_400()
        {
            var postIncomeDto = new PostIncomeDto("a", 20, new DateTime());
            int userId = 1;

            _incomeRepositoryMock.Setup(repo => repo.AddIncome(It.IsAny<Income>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount_Of_Characters));

            var result = await _sut.CreateIncome(postIncomeDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Amount_Of_Characters in the title.", objectResult.Value);
        }

        [Fact]
        public async Task CreateIncome_Invalid_Amount_Returns_400()
        {
            var postIncomeDto = new PostIncomeDto("testTitle", 0, new DateTime());
            int userId = 1;

            _incomeRepositoryMock.Setup(repo => repo.AddIncome(It.IsAny<Income>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount));

            var result = await _sut.CreateIncome(postIncomeDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Amount", objectResult.Value);
        }

        [Fact]
        public async Task CreateIncome_Valid_Input_Returns_201()
        {
            int userId = 1;

            var postIncomeDto = new PostIncomeDto("testTitle", 20, new DateTime());

            var getIncomeDto = new GetIncomeDto();
            getIncomeDto.Id = 1;
            getIncomeDto.Date = new DateTime();
            getIncomeDto.Title = "testTitle";
            getIncomeDto.Amount = 20;
            getIncomeDto.ReOccuring = false;

            var income = new Income("testTitle", 20, new DateTime());
            income.Id = 1;
            income.ReOccuring = false;
            income.MonthAnalysis = new List<MonthAnalysis>();
            income.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");

            _incomeRepositoryMock.Setup(repo => repo.AddIncome(It.IsAny<Income>(), userId))
                                 .ReturnsAsync((income, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<GetIncomeDto>(income))
                       .Returns(getIncomeDto);

            var result = await _sut.CreateIncome(postIncomeDto, userId);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.NotNull(createdAtResult.Value);
            Assert.IsType<GetIncomeDto>(createdAtResult.Value);
        }

        [Fact]
        public async Task CreateIncome_Exception_Returns_500()
        {
            int userId = 1;
            var postIncomeDto = new PostIncomeDto("testTitle", 20, new DateTime());

            _incomeRepositoryMock.Setup(repo => repo.AddIncome(It.IsAny<Income>(), userId))
                                 .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.CreateIncome(postIncomeDto, userId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetIncome-Tests
        [Fact]
        public async Task GetIncome_UnauthorizedUser_Returns_401()
        {
            int unauthorizedUserId = 2;
            var income = new Income("testIncome", 500, new DateTime());
            income.Id = 1;
            income.ReOccuring = false;
            income.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            income.User.Id = 1;

            _incomeRepositoryMock.Setup(x => x.GetIncome(1))
                                 .ReturnsAsync(income);

            _mockMapper.Setup(x => x.Map<GetIncomeDto>(income))
                       .Returns(new GetIncomeDto());

           
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

            var result = await _sut.GetIncome(1);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetIncome_Income_NotFound_Returns_404()
        {
            _incomeRepositoryMock.Setup(x => x.GetIncome(1))
                               .ReturnsAsync(null as Income);

            var result = await _sut.GetIncome(1);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("Income with Id: 1, could not be found.", objectResult.Value);
        }

        [Fact]
        public async Task GetIncome_Income_Found_Returns_200()
        {
            var income = new Income("testIncome", 500, new DateTime());
            income.Id = 1;
            income.ReOccuring = false;
            income.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            income.User.Id = 1;

            _incomeRepositoryMock.Setup(x => x.GetIncome(1))
                                 .ReturnsAsync(income);

            _mockMapper.Setup(x => x.Map<GetIncomeDto>(income))
                       .Returns(new GetIncomeDto());

            var result = await _sut.GetIncome(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetIncomeDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetIncome_Exception_Returns_500()
        {
            _incomeRepositoryMock.Setup(repo => repo.GetIncome(1))
                                 .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.GetIncome(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUserIncomes-Tests
        [Fact]
        public async Task GetUserIncomes_UnauthorizedUser_Returns_401()
        {
            int userId = 1;
            int unauthorizedUserId = 2;
            var mockIncomeList = new List<Income>
            {
                new Income("income1", 200, new DateTime()) { Id = 1, ReOccuring = false, User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash"){ Id = 1 } },
                new Income("income2", 200, new DateTime()) { Id = 2, ReOccuring = false, User = new User("testUsera", Encoding.UTF8.GetBytes("testSalt"), "testHash"){ Id = 2 } },
            };

            _incomeRepositoryMock.Setup(repo => repo.GetUserIncomes(userId))
                                 .ReturnsAsync(mockIncomeList);

            _mockMapper.Setup(x => x.Map<List<GetIncomeDto>>(It.IsAny<List<Income>>()))
                       .Returns(new List<GetIncomeDto>());


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

            var result = await _sut.GetUserIncomes(userId);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetUserIncomes_Returns_List_Of_Incomes_Returns_200()
        {
            int userId = 1;
            var mockIncomeList = new List<Income>
            {
                new Income("income1", 200, new DateTime()) { Id = 1, ReOccuring = false },
                new Income("income2", 200, new DateTime()) { Id = 2, ReOccuring = false },
            };

            _incomeRepositoryMock.Setup(repo => repo.GetUserIncomes(userId))
                                 .ReturnsAsync(mockIncomeList);

            _mockMapper.Setup(x => x.Map<List<GetIncomeDto>>(It.IsAny<List<Income>>()))
                       .Returns(new List<GetIncomeDto>());

            var result = await _sut.GetUserIncomes(userId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<List<GetIncomeDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetUserIncomes_InternalServerError_Returns_500()
        {
            int userId = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetUserIncomes(userId))
                                 .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.GetUserIncomes(userId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region EditIncome-Tests
        [Fact]
        public async Task EditIncome_UnAuthorized_Returns_401()
        {
            int incomeId = 1;
            int unauthorizedUserId = 2;
            var postIncomeDto = new PostIncomeDto("testTitle", 20, new DateTime());
            var editedIncome = new Income("testTitle", 20, new DateTime());
            editedIncome.Id = incomeId;
            editedIncome.ReOccuring = false;
            editedIncome.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            editedIncome.User.Id = 1;

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(false);

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(editedIncome);

            _mockMapper.Setup(x => x.Map<GetIncomeDto>(editedIncome))
                       .Returns(new GetIncomeDto());

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

            var result = await _sut.EditIncome(postIncomeDto, incomeId);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task EditIncome_Valid_Input_Returns_200()
        {
            int incomeId = 1;
            var postIncomeDto = new PostIncomeDto("testTitle", 20, new DateTime());
            var editedIncome = new Income("testTitle", 20, new DateTime());
            editedIncome.Id = incomeId;
            editedIncome.ReOccuring = false;
            editedIncome.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            editedIncome.User.Id = 1;

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(true);

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId)) 
                                 .ReturnsAsync(editedIncome);

            _incomeRepositoryMock.Setup(repo => repo.EditIncome(It.IsAny<Income>(), incomeId))
                                 .ReturnsAsync((editedIncome, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<GetIncomeDto>(editedIncome))
                       .Returns(new GetIncomeDto());

            var result = await _sut.EditIncome(postIncomeDto, incomeId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<GetIncomeDto>(okResult.Value);
        }

        [Fact]
        public async Task EditIncome_IncomeNotFound_Returns_404()
        {
            int incomeId = 1;

            _incomeRepositoryMock.Setup(repo => repo.EditIncome(It.IsAny<Income>(), incomeId))
                                 .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.EditIncome(new PostIncomeDto("testTitle", 20, new DateTime()), incomeId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Income not found", notFoundResult.Value);
        }

        [Fact]
        public async Task EditIncome_InvalidAmountOfCharacters_Returns_400()
        {
            int incomeId = 1;
            var previousIncome = new Income("testTitle", 20, new DateTime());
            previousIncome.Id = incomeId;
            previousIncome.ReOccuring = false;
            previousIncome.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            previousIncome.User.Id = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(previousIncome);

            _incomeRepositoryMock.Setup(repo => repo.EditIncome(It.IsAny<Income>(), incomeId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount_Of_Characters));

            var result = await _sut.EditIncome(new PostIncomeDto("testTitle", 20, new DateTime()), incomeId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid_Amount_Of_Characters in the title.", badRequestResult.Value);
        }

        [Fact]
        public async Task EditIncome_InvalidAmount_Returns_400()
        {
            int incomeId = 1;
            var previousIncome = new Income("testTitle", 20, new DateTime());
            previousIncome.Id = incomeId;
            previousIncome.ReOccuring = false;
            previousIncome.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            previousIncome.User.Id = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(previousIncome);

            _incomeRepositoryMock.Setup(repo => repo.EditIncome(It.IsAny<Income>(), incomeId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount));

            var result = await _sut.EditIncome(new PostIncomeDto("testTitle", 20, new DateTime()), incomeId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid_Amount", badRequestResult.Value);
        }

        [Fact]
        public async Task EditIncome_InternalServerError_Returns_500()
        {
            int incomeId = 1;
            var previousIncome = new Income("testTitle", 20, new DateTime());
            previousIncome.Id = incomeId;
            previousIncome.ReOccuring = false;
            previousIncome.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            previousIncome.User.Id = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(previousIncome);

            _incomeRepositoryMock.Setup(repo => repo.EditIncome(It.IsAny<Income>(), incomeId))
                                 .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.EditIncome(new PostIncomeDto("testTitle", 20, new DateTime()), incomeId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region DeleteIncome-Tests
        [Fact]
        public async Task DeleteIncome_UnAuthorized_Returns_401()
        {
            int incomeId = 1;
            int unauthorizedUserId = 2;
            var deleteIncome = new Income("testTitle", 20, new DateTime());
            deleteIncome.Id = incomeId;
            deleteIncome.ReOccuring = false;
            deleteIncome.User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash");
            deleteIncome.User.Id = 1;

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(false);

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(deleteIncome);           

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

            var result = await _sut.DeleteIncome(incomeId);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task DeleteIncome_ValidId_Returns_204()
        {
            int incomeId = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(new Income("asd", 20, new DateTime()) { Id = incomeId, User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 } } );

            _incomeRepositoryMock.Setup(repo => repo.DeleteIncome(incomeId))
                                 .ReturnsAsync(new Income("asd", 20, new DateTime()) { Id = incomeId });

            var result = await _sut.DeleteIncome(incomeId);

            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteIncome_Income_NotFound_Returns_404()
        {
            int incomeId = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(new Income("asd", 20, new DateTime()) { Id = incomeId, User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 } });

            _incomeRepositoryMock.Setup(repo => repo.DeleteIncome(incomeId))
                                 .ReturnsAsync(null as Income);

            var result = await _sut.DeleteIncome(incomeId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal($"Income with Id: {incomeId}, could not be found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteIncome_InternalServerError_Returns_500()
        {
            int incomeId = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetIncome(incomeId))
                                 .ReturnsAsync(new Income("asd", 20, new DateTime()) { Id = incomeId, User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash") { Id = 1 } });

            _incomeRepositoryMock.Setup(repo => repo.DeleteIncome(incomeId))
                                 .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.DeleteIncome(incomeId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUserIncomesByMonth-Tests
        [Fact]
        public async Task GetUserIncomesByMonth_UnauthorizedUser_Returns_401()
        {
            int userId = 1;
            int unauthorizedUserId = 2;
            var mockIncomeList = new List<Income>
            {
                new Income("income1", 200, new DateTime()) { Id = 1, ReOccuring = false, User = new User("testUser", Encoding.UTF8.GetBytes("testSalt"), "testHash"){ Id = 1 } },
                new Income("income2", 200, new DateTime()) { Id = 2, ReOccuring = false, User = new User("testUsera", Encoding.UTF8.GetBytes("testSalt"), "testHash"){ Id = 2 } },
            };

            _incomeRepositoryMock.Setup(repo => repo.GetUserIncomesByMonth(userId, 2023, 12))
                                 .ReturnsAsync(mockIncomeList);

            _mockMapper.Setup(x => x.Map<List<GetIncomeDto>>(It.IsAny<List<Income>>()))
                       .Returns(new List<GetIncomeDto>());


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

            var result = await _sut.GetUserIncomesByMonth(userId, 2023, 12);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetUserIncomesByMonth_Returns_List_Of_Incomes_Returns_200()
        {
            int userId = 1;
            var dateTime = new DateTime(2023, 12, 1);

            var mockIncomeList = new List<Income>
            {
                new Income("income1", 200, dateTime) { Id = 1, ReOccuring = false },
                new Income("income2", 200, dateTime) { Id = 2, ReOccuring = false },
            };

            _incomeRepositoryMock.Setup(repo => repo.GetUserIncomesByMonth(userId, 2023, 12))
                                 .ReturnsAsync(mockIncomeList);

            _mockMapper.Setup(x => x.Map<List<GetIncomeDto>>(It.IsAny<List<Income>>()))
                       .Returns(new List<GetIncomeDto>());

            var result = await _sut.GetUserIncomesByMonth(userId, 2023, 12);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<List<GetIncomeDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetUserIncomesByMonth_InternalServerError_Returns_500()
        {
            int userId = 1;

            _incomeRepositoryMock.Setup(repo => repo.GetUserIncomesByMonth(userId, 2022, 3))
                                 .ThrowsAsync(new Exception("Simulated repository exception"));

            var result = await _sut.GetUserIncomesByMonth(userId, 2022, 3);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
    }
}