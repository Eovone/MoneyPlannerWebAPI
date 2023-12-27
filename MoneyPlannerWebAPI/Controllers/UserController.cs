using AutoMapper;
using Entity;
using Infrastructure.Repositories.UserRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.UserDto;
using MoneyPlannerWebAPI.Utilities;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;
        private readonly IAuthorizationHelper _authorizationHelper;
        public UserController(IMapper mapper, IUserRepository repository, ILogger<UserController> logger, IAuthorizationHelper authorizationHelper)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _authorizationHelper = authorizationHelper;
        }

        [HttpPost]     
        public async Task<ActionResult> CreateUser(PostUserDto postUserDto)
        {
            try
            {
                var (createdUser, validationStatus) = await _repository.AddUser(postUserDto.Username,postUserDto.Password);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating User: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Invalid_Password) return BadRequest(validationStatus.ToString());
                if (validationStatus == ValidationStatus.Username_Already_Exist) return BadRequest(validationStatus.ToString());
              
                _logger.LogInformation($"User: {createdUser!.Username} with Id: {createdUser.Id} was successfully created.");
                return Ok("User Successfully created");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to create User.");
                return StatusCode(500, "Internal Server Error");
            }

        }
        
        [HttpGet("{id}")]
        [Authorize]
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

                if (!_authorizationHelper.IsUserAuthorized(User, user.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${user.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
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
