using AutoMapper;
using Entity;
using Infrastructure.Repositories.BudgetPlanningRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyPlannerWebAPI.Controllers;
using MoneyPlannerWebAPI.DTO.BudgetPlanningDto;
using MoneyPlannerWebAPI.Utilities;
using Moq;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Tests.Controllers
{
    public class BudgetPlanningControllerTests
    {
        private Mock<IBudgetPlanningRepository> _budgetRepositoryMock;
        private Mock<ILogger<BudgetPlanningController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private Mock<IAuthorizationHelper> _authorizationHelperMock;
        private BudgetPlanningController _sut;
        public BudgetPlanningControllerTests()
        {
            _budgetRepositoryMock = new Mock<IBudgetPlanningRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<BudgetPlanningController>>();
            _authorizationHelperMock = new Mock<IAuthorizationHelper>();

            _authorizationHelperMock.Setup(helper => helper.IsUserAuthorized(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
                                    .Returns(true);

            _sut = new BudgetPlanningController(_mockMapper.Object, _budgetRepositoryMock.Object, _mockLogger.Object, _authorizationHelperMock.Object);
        }
        #region CreateBudgetPlan-Tests
        [Fact]
        public async Task CreateBudgetPlan_UnauthorizedUser_Returns_401()
        {
            var postBudgetPlanDto = new PostBudgetPlanningDto();
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

            var result = await _sut.CreateBudgetPlan(postBudgetPlanDto, unauthorizedUserId);

            Assert.NotNull(result);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task CreateBudgetPlan_BudgetPlan_Doesnt_Exist_Returns_404()
        {
            var postBudgetPlanDto = new PostBudgetPlanningDto();
            postBudgetPlanDto.BudgetPlanItemsDto = new List<PostBudgetPlanningItemDto>
            { new PostBudgetPlanningItemDto { Title = "asdf", Amount = 2, IsIncome = false } };
            postBudgetPlanDto.SummaryAmount = 2;

            int userId = 1;

            _budgetRepositoryMock.Setup(repo => repo.CreateBudgetPlan(It.IsAny<BudgetPlan>(), It.IsAny<List<BudgetPlanItem>>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Not_Found));

            var result = await _sut.CreateBudgetPlan(postBudgetPlanDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("BudgetPlan Not Found", objectResult.Value);
        }

        [Fact]
        public async Task CreateBudgetPlan_TitleOfAnItem_Too_Short_Returns_400()
        {
            var postBudgetPlanDto = new PostBudgetPlanningDto();
            postBudgetPlanDto.BudgetPlanItemsDto = new List<PostBudgetPlanningItemDto>();            

            int userId = 1;

            _budgetRepositoryMock.Setup(repo => repo.CreateBudgetPlan(It.IsAny<BudgetPlan>(), It.IsAny<List<BudgetPlanItem>>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount_Of_Characters));

            var result = await _sut.CreateBudgetPlan(postBudgetPlanDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Amount_Of_Characters in the title.", objectResult.Value);
        }

        [Fact]
        public async Task CreateBudgetPlan_Invalid_SummaryAmount_Returns_400()
        {
            var postBudgetPlanDto = new PostBudgetPlanningDto();
            postBudgetPlanDto.BudgetPlanItemsDto = new List<PostBudgetPlanningItemDto>();

            int userId = 1;

            _budgetRepositoryMock.Setup(repo => repo.CreateBudgetPlan(It.IsAny<BudgetPlan>(), It.IsAny<List<BudgetPlanItem>>(), userId))
                                 .ReturnsAsync((null, ValidationStatus.Invalid_Amount));

            var result = await _sut.CreateBudgetPlan(postBudgetPlanDto, userId);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Invalid_Amount", objectResult.Value);
        }

        [Fact]
        public async Task CreateBudgetPlan_Valid_Input_Returns_201()
        {
            int userId = 1;

            var postBudgetPlanDto = new PostBudgetPlanningDto();
            postBudgetPlanDto.BudgetPlanItemsDto = new List<PostBudgetPlanningItemDto>
            { new PostBudgetPlanningItemDto { Title = "asdf", Amount = 2, IsIncome = false } };
            postBudgetPlanDto.SummaryAmount = 2;

            var getBudgetPlanDto = new GetBudgetPlanningDto();
            getBudgetPlanDto.BudgetPlanItemsDto = new List<GetBudgetPlanningItemDto>
            { new GetBudgetPlanningItemDto { Title = "asdf", Amount = 2, IsIncome = false } };
            getBudgetPlanDto.SummaryAmount = 2;
            getBudgetPlanDto.UserId = userId;
            getBudgetPlanDto.Id = 1;

            var budgetPlan = new BudgetPlan();
            budgetPlan.Id = 1;
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem> 
            { new BudgetPlanItem { Id = 1, Amount = 2, IsIncome = false, Title = "asdf" } };
            budgetPlan.SummaryAmount = 2;
            budgetPlan.UserId = userId;

            _mockMapper.Setup(x => x.Map<BudgetPlan>(postBudgetPlanDto))
                       .Returns(budgetPlan);

            _budgetRepositoryMock.Setup(repo => repo.CreateBudgetPlan(It.IsAny<BudgetPlan>(), It.IsAny<List<BudgetPlanItem>>(), userId))
                                 .ReturnsAsync((budgetPlan, ValidationStatus.Success));

            _mockMapper.Setup(x => x.Map<List<GetBudgetPlanningItemDto>>(budgetPlan.BudgetPlanItems))
                       .Returns(getBudgetPlanDto.BudgetPlanItemsDto);

            _mockMapper.Setup(x => x.Map<GetBudgetPlanningDto>(budgetPlan))
                       .Returns(getBudgetPlanDto);

            var result = await _sut.CreateBudgetPlan(postBudgetPlanDto, userId);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.NotNull(createdAtResult.Value);
            Assert.IsType<GetBudgetPlanningDto>(createdAtResult.Value);
        }

        [Fact]
        public async Task CreateBudgetPlan_Exception_Returns_500()
        {
            var postBudgetPlanDto = new PostBudgetPlanningDto();
            postBudgetPlanDto.BudgetPlanItemsDto = new List<PostBudgetPlanningItemDto>();

            int userId = 1;

            _budgetRepositoryMock.Setup(repo => repo.CreateBudgetPlan(It.IsAny<BudgetPlan>(), It.IsAny<List<BudgetPlanItem>>(), userId))
                                 .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.CreateBudgetPlan(postBudgetPlanDto, userId);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetBudgetPlan-Tests
        [Fact]
        public async Task GetBudgetPlan_UnauthorizedUser_Returns_401()
        {
            int unauthorizedUserId = 2;

            var budgetPlan = new BudgetPlan();
            budgetPlan.Id = 1;
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem>
            { new BudgetPlanItem { Id = 1, Amount = 2, IsIncome = false, Title = "asdf" } };
            budgetPlan.SummaryAmount = 2;
            budgetPlan.UserId = 1;

            _budgetRepositoryMock.Setup(x => x.GetBudgetPlan(1))
                                 .ReturnsAsync(budgetPlan);        

            _mockMapper.Setup(x => x.Map<GetBudgetPlanningDto>(budgetPlan))
                       .Returns(new GetBudgetPlanningDto());

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

            var result = await _sut.GetBudgetPlan(1);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetBudgetPlan_BudgetPlan_NotFound_Returns_404()
        {
            _budgetRepositoryMock.Setup(x => x.GetBudgetPlan(1))
                                 .ReturnsAsync(null as BudgetPlan);

            var result = await _sut.GetBudgetPlan(1);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("BudgetPlan with Id: 1, could not be found.", objectResult.Value);
        }

        [Fact]
        public async Task GetBudgetPlan_BudgetPlan_Found_Returns_200()
        {
            int userId = 1;

            var budgetPlan = new BudgetPlan();
            budgetPlan.Id = 1;
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem>
            { new BudgetPlanItem { Id = 1, Amount = 2, IsIncome = false, Title = "asdf" } };
            budgetPlan.SummaryAmount = 2;
            budgetPlan.UserId = userId;

            _budgetRepositoryMock.Setup(x => x.GetBudgetPlan(1))
                                 .ReturnsAsync(budgetPlan);

            _mockMapper.Setup(x => x.Map<GetBudgetPlanningDto>(budgetPlan))
                       .Returns(new GetBudgetPlanningDto());

            var result = await _sut.GetBudgetPlan(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetBudgetPlanningDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetBudgetPlan_Exception_Returns_500()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPlan(1))
                                 .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.GetBudgetPlan(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
        #region GetUserBudgetPlan-Tests
        [Fact]
        public async Task GetUserBudgetPlan_UnauthorizedUser_Returns_401()
        {
            int unauthorizedUserId = 2;
            int userId = 1;

            var budgetPlan = new BudgetPlan();
            budgetPlan.Id = 1;
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem>
            { new BudgetPlanItem { Id = 1, Amount = 2, IsIncome = false, Title = "asdf" } };
            budgetPlan.SummaryAmount = 2;
            budgetPlan.UserId = userId;

            _budgetRepositoryMock.Setup(x => x.GetUserBudgetPlan(userId))
                                 .ReturnsAsync(budgetPlan);

            _mockMapper.Setup(x => x.Map<GetBudgetPlanningDto>(budgetPlan))
                       .Returns(new GetBudgetPlanningDto());

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

            var result = await _sut.GetUserBudgetPlan(userId);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetUserBudgetPlan_BudgetPlan_NotFound_Returns_404()
        {
            _budgetRepositoryMock.Setup(x => x.GetUserBudgetPlan(1))
                                 .ReturnsAsync(null as BudgetPlan);

            var result = await _sut.GetUserBudgetPlan(1);

            Assert.NotNull(result);
            var objectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("BudgetPlan for User with Id: 1, could not be found.", objectResult.Value);
        }

        [Fact]
        public async Task GetUserBudgetPlan_BudgetPlan_Found_Returns_200()
        {
            int userId = 1;

            var budgetPlan = new BudgetPlan();
            budgetPlan.Id = 1;
            budgetPlan.BudgetPlanItems = new List<BudgetPlanItem>
            { new BudgetPlanItem { Id = 1, Amount = 2, IsIncome = false, Title = "asdf" } };
            budgetPlan.SummaryAmount = 2;
            budgetPlan.UserId = userId;

            _budgetRepositoryMock.Setup(x => x.GetUserBudgetPlan(1))
                                 .ReturnsAsync(budgetPlan);

            _mockMapper.Setup(x => x.Map<GetBudgetPlanningDto>(budgetPlan))
                       .Returns(new GetBudgetPlanningDto());

            var result = await _sut.GetUserBudgetPlan(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);
            Assert.IsType<GetBudgetPlanningDto>(objectResult.Value);
        }

        [Fact]
        public async Task GetUserBudgetPlan_Exception_Returns_500()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetUserBudgetPlan(1))
                                 .ThrowsAsync(new Exception("Simulated exception"));

            var result = await _sut.GetUserBudgetPlan(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal Server Error", objectResult.Value);
        }
        #endregion
    }
}