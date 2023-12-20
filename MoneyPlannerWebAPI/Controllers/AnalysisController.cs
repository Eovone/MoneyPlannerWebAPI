using AutoMapper;
using Entity;
using Infrastructure.Repositories.AnalysisRepo;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.AnalysisDto;
using MoneyPlannerWebAPI.DTO.IncomeDto;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAnalysisRepository _repository;
        private readonly ILogger<AnalysisController> _logger;
        public AnalysisController(IMapper mapper, IAnalysisRepository repository, ILogger<AnalysisController> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetMonthlyAnalysisDto>> CreateMonthAnalysis(PostMonthlyAnalysisDto postMonthlyAnalysisDto, int userId)
        {
            var monthAnalysis = await _repository.CreateMonthAnalysis(postMonthlyAnalysisDto.Month, postMonthlyAnalysisDto.Year, userId);

            var getMonthAnalysis = _mapper.Map<GetMonthlyAnalysisDto>(monthAnalysis);
            return Ok(getMonthAnalysis);
        }
    }
}
