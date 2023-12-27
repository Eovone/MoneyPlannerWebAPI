using AutoMapper;
using Entity;
using Infrastructure.Repositories.IncomeRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.IncomeDto;
using MoneyPlannerWebAPI.Utilities;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IncomeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IIncomeRepository _repository;
        private readonly ILogger<IncomeController> _logger;  
        private readonly IAuthorizationHelper _authorizationHelper;
        public IncomeController(IMapper mapper, IIncomeRepository repository, ILogger<IncomeController> logger, IAuthorizationHelper authorizationHelper)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _authorizationHelper = authorizationHelper;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetIncomeDto>> CreateIncome(PostIncomeDto postIncomeDto, int userId)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var (createdIncome, validationStatus) = await _repository.AddIncome(_mapper.Map<Income>(postIncomeDto), userId);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating Income: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("User Not Found");
                if (validationStatus == ValidationStatus.Invalid_Amount_Of_Characters) return BadRequest($"{validationStatus} in the title.");
                if (validationStatus == ValidationStatus.Invalid_Amount) return BadRequest($"{validationStatus}");
               
                var getIncomeDto = _mapper.Map<GetIncomeDto>(createdIncome);
                _logger.LogInformation($"Income with Id: {getIncomeDto.Id} was successfully created.");
                return CreatedAtAction("GetIncome", new { id = getIncomeDto.Id }, getIncomeDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to create Income.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetIncomeDto>> GetIncome(int id)
        {
            try
            {                
                var income = await _repository.GetIncome(id);
                if (income == null)
                {
                    _logger.LogError($"Income with Id: {id}, does not exist.");
                    return NotFound($"Income with Id: {id}, could not be found.");
                }

                if (!_authorizationHelper.IsUserAuthorized(User, income.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${income.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                _logger.LogInformation($"Income with Id: {income.Id}, fetched successfully.");
                return Ok(_mapper.Map<GetIncomeDto>(income));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Income with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("User/{userId}")]
        public async Task<ActionResult<List<GetIncomeDto>>> GetUserIncomes(int userId)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var incomeList = await _repository.GetUserIncomes(userId);
           
                _logger.LogInformation($"Incomes for user with Id: {userId}, fetched successfully.");
                return Ok(_mapper.Map<List<GetIncomeDto>>(incomeList));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Incomes for user with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GetIncomeDto>> EditIncome(PostIncomeDto postIncomeDto, int id)
        {
            try
            {
                var originalIncome = await _repository.GetIncome(id);
                if (originalIncome == null) return NotFound("Income not found");
                if (!_authorizationHelper.IsUserAuthorized(User, originalIncome.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${originalIncome.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var (editedIncome, validationStatus) = await _repository.EditIncome(_mapper.Map<Income>(postIncomeDto), id);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating Income: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("Income not found");
                if (validationStatus == ValidationStatus.Invalid_Amount_Of_Characters) return BadRequest($"{validationStatus} in the title.");
                if (validationStatus == ValidationStatus.Invalid_Amount) return BadRequest($"{validationStatus}");
             
                _logger.LogInformation($"Income with Id: {id}, edited successfully.");
                return Ok(_mapper.Map<GetIncomeDto>(editedIncome));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to edit Income: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIncome(int id)
        {
            try
            {
                var originalIncome = await _repository.GetIncome(id);
                if (originalIncome == null) return NotFound("Income not found");
                if (!_authorizationHelper.IsUserAuthorized(User, originalIncome.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${originalIncome.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var deletedIncome = await _repository.DeleteIncome(id);
                if (deletedIncome == null)
                {
                    _logger.LogError($"Income with Id: {id}, does not exist.");
                    return NotFound($"Income with Id: {id}, could not be found.");
                }
                _logger.LogInformation($"Income with Id: {id}, deleted successfully.");
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to delete Income: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("User/{userId}/Year/{year}/Month/{monthNumber}")]
        public async Task<ActionResult<List<GetIncomeDto>>> GetUserIncomesByMonth(int userId, int year, int monthNumber)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var incomeList = await _repository.GetUserIncomesByMonth(userId, year, monthNumber);

                _logger.LogInformation($"Incomes for user with Id: {userId}, fetched successfully.");
                return Ok(_mapper.Map<List<GetIncomeDto>>(incomeList));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Incomes for user with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
