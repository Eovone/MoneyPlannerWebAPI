using AutoMapper;
using Entity;
using Infrastructure.Repositories.IncomeRepo;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.IncomeDto;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IIncomeRepository _repository;
        private readonly ILogger<IncomeController> _logger;
        public IncomeController(IMapper mapper, IIncomeRepository repository, ILogger<IncomeController> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetIncomeDto>> CreateIncome(PostIncomeDto postIncomeDto, int userId)
        {
            try
            {
                var createdIncome = await _repository.AddIncome(_mapper.Map<Income>(postIncomeDto), userId);
                if (createdIncome == null)
                {
                    _logger.LogWarning("Trying to create an Income, but one or more of the properties are invalid.");
                    return BadRequest("Invalid property/ies.");
                }
                // returna objektet GetIncomeDto med mapper istället som är gjort i USER.
                _logger.LogInformation($"Income with Id: {createdIncome.Id} was successfully created.");
                return CreatedAtAction("GetIncome", new { id = createdIncome.Id }, createdIncome.Id);
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
                    _logger.LogWarning($"Income with Id: {id}, does not exist.");
                    return NotFound($"Income with Id: {id}, could not be found.");
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
                var incomeList = await _repository.GetUserIncomes(userId);
                if (incomeList == null)
                {
                    _logger.LogWarning($"Incomes for user with Id: {userId}, does not exist.");
                    return NotFound($"Incomes for user with Id: {userId}, could not be found.");
                }
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
