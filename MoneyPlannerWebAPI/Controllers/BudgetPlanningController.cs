using AutoMapper;
using Entity;
using Infrastructure.Repositories.BudgetPlanningRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.BudgetPlanningDto;
using MoneyPlannerWebAPI.Utilities;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetPlanningController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBudgetPlanningRepository _repository;
        private readonly ILogger<BudgetPlanningController> _logger;
        private readonly IAuthorizationHelper _authorizationHelper;
        public BudgetPlanningController(IMapper mapper, IBudgetPlanningRepository repository, ILogger<BudgetPlanningController> logger, IAuthorizationHelper authorizationHelper)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _authorizationHelper = authorizationHelper;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetBudgetPlanningDto>> CreateBudgetPlan(PostBudgetPlanningDto postBudgetPlanningDto, int userId)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var budgetPlanItems = _mapper.Map<List<BudgetPlanItem>>(postBudgetPlanningDto.BudgetPlanItemsDto);
                var budgetPlan = _mapper.Map<BudgetPlan>(postBudgetPlanningDto);

                var createdBudgetPlan = await _repository.CreateBudgetPlan(budgetPlan, budgetPlanItems, userId);

                var getBudgetPlan = _mapper.Map<GetBudgetPlanningDto>(createdBudgetPlan);
                _logger.LogInformation($"BudgetPlan with Id: {budgetPlan.Id} was successfully created.");
                return CreatedAtAction("GetBudgetPlan", new { id = getBudgetPlan.Id }, getBudgetPlan);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to create BudgetPlan.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetBudgetPlanningDto>> GetBudgetPlan(int id)
        {
            try
            {
                var budgetPlan = await _repository.GetBudgetPlan(id);
                if (budgetPlan == null)
                {
                    _logger.LogError($"BudgetPlan with Id: {id}, does not exist.");
                    return NotFound($"BudgetPlan with Id: {id}, could not be found.");
                }

                if (!_authorizationHelper.IsUserAuthorized(User, budgetPlan.UserId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${budgetPlan.UserId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var getBudgetPlanItems = _mapper.Map<List<GetBudgetPlanningItemDto>>(budgetPlan.BudgetPlanItems);
                var getBudgetPlan = _mapper.Map<GetBudgetPlanningDto>(budgetPlan);
                getBudgetPlan.BudgetPlanItemsDto = getBudgetPlanItems;

                _logger.LogInformation($"BudgetPlan with Id: {budgetPlan.Id}, fetched successfully.");
                return Ok(getBudgetPlan);
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get BudgetPlan with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("User/{userId}")]
        public async Task<ActionResult<GetBudgetPlanningDto>> GetUserBudgetPlan(int userId)
        {
            try
            {
                var budgetPlan = await _repository.GetUserBudgetPlan(userId);
                if (budgetPlan == null)
                {
                    _logger.LogError($"BudgetPlan for User with Id: {userId}, does not exist.");
                    return NotFound($"BudgetPlan for User with Id: {userId}, could not be found.");
                }

                if (!_authorizationHelper.IsUserAuthorized(User, budgetPlan.UserId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${budgetPlan.UserId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var getBudgetPlanItems = _mapper.Map<List<GetBudgetPlanningItemDto>>(budgetPlan.BudgetPlanItems);
                var getBudgetPlan = _mapper.Map<GetBudgetPlanningDto>(budgetPlan);
                getBudgetPlan.BudgetPlanItemsDto = getBudgetPlanItems;

                _logger.LogInformation($"BudgetPlan with Id: {budgetPlan.Id}, fetched successfully.");
                return Ok(getBudgetPlan);
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get BudgetPlan with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}