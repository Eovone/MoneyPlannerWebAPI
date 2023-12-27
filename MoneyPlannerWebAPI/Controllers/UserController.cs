using AutoMapper;
using Infrastructure.Repositories.UserRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.UserDto;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;
        public UserController(IMapper mapper, IUserRepository repository, ILogger<UserController> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<GetUserDto>> CreateUser(PostUserDto postUserDto)
        {
            try
            {
                var (createdUser, validationStatus) = await _repository.AddUser(postUserDto.Username,postUserDto.Password);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating User: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Invalid_Password) return BadRequest(validationStatus.ToString());
                if (validationStatus == ValidationStatus.Username_Already_Exist) return BadRequest(validationStatus.ToString());
              
                var getUserDto = _mapper.Map<GetUserDto>(createdUser);
                _logger.LogInformation($"User: {getUserDto.Username} with Id: {getUserDto.Id} was successfully created.");
                return CreatedAtAction("GetUser", new { id = getUserDto.Id }, getUserDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to create User.");
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetUserDto>> GetUser(int id)
        {
            try
            {
                var user = await _repository.GetUser(id);
                if (user == null)
                {
                    _logger.LogError("User Not Found");
                    return NotFound("User Not Found");
                }
                _logger.LogInformation($"User with Id: {user.Id}, fetched successfully.");
                return Ok(_mapper.Map<GetUserDto>(user));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error trying to get user with Id: {id}.");
                return StatusCode(500, "Internal Server Error");
            }
        }       
    }
}
