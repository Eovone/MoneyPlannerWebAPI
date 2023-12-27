using AutoMapper;
using Infrastructure.Repositories.AnalysisRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.AnalysisDto;
using MoneyPlannerWebAPI.Utilities;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalysisController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAnalysisRepository _repository;
        private readonly ILogger<AnalysisController> _logger;
        private readonly IAuthorizationHelper _authorizationHelper;
        public AnalysisController(IMapper mapper, IAnalysisRepository repository, ILogger<AnalysisController> logger, IAuthorizationHelper authorizationHelper)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _authorizationHelper = authorizationHelper;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetMonthlyAnalysisDto>> CreateMonthAnalysis(PostMonthlyAnalysisDto postMonthlyAnalysisDto, int userId)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var (monthAnalysis, validationStatus) = await _repository.CreateMonthAnalysis(postMonthlyAnalysisDto.Month, postMonthlyAnalysisDto.Year, userId);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating Income: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("User Not Found");
                if (validationStatus == ValidationStatus.No_Data_To_Make_Analysis) return BadRequest($"{validationStatus}");

                var getMonthAnalysisDto = _mapper.Map<GetMonthlyAnalysisDto>(monthAnalysis);
                _logger.LogInformation($"MonthAnalysis with Id: {getMonthAnalysisDto.Id} was successfully created.");
                return CreatedAtAction("GetMonthAnalysis", new { id = getMonthAnalysisDto.Id }, getMonthAnalysisDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to create MonthAnalysis.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetMonthlyAnalysisDto>> GetMonthAnalysis(int id)
        {
            try
            {
                var monthAnalysis = await _repository.GetMonthAnalysis(id);
                if (monthAnalysis == null)
                {
                    _logger.LogError($"MonthAnalysis with Id: {id}, does not exist.");
                    return NotFound($"MonthAnalysis with Id: {id}, could not be found.");
                }

                if (!_authorizationHelper.IsUserAuthorized(User, monthAnalysis.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${monthAnalysis.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                _logger.LogInformation($"MonthAnalysis with Id: {monthAnalysis.Id}, fetched successfully.");
                return Ok(_mapper.Map<GetMonthlyAnalysisDto>(monthAnalysis));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get MonthAnalysis with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("User/{userId}/Year/{year}/Month/{monthNumber}")]
        public async Task<ActionResult<GetMonthlyAnalysisDto>> GetMonthAnalysisByMonth(int userId, int year, int monthNumber)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var monthAnalysis = await _repository.GetMonthAnalysisByMonth(monthNumber, year, userId);
                if (monthAnalysis == null)
                {
                    _logger.LogError($"MonthAnalysis for month: {monthNumber}, does not exist.");
                    return NotFound($"MonthAnalysis for month: {monthNumber}, does not exist.");
                }
                _logger.LogInformation($"MonthAnalysis with Id: {monthAnalysis.Id}, fetched successfully.");
                return Ok(_mapper.Map<GetMonthlyAnalysisDto>(monthAnalysis));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get MonthAnalysis with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
