using AutoMapper;
using Entity;
using Infrastructure.Repositories.UserRepo;
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
                var createdUser = await _repository.AddUser(postUserDto.Username,postUserDto.Password);
                if (createdUser == null)
                {
                    _logger.LogWarning("Trying to create a User, but username already exist or password was invalid");
                    return BadRequest("Username already exists or password was invalid.");
                }
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
                    _logger.LogWarning($"User with Id: {id}, does not exist.");
                    return NotFound($"User with Id: {id}, could not be found.");
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
