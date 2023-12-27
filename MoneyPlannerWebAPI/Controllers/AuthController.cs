using Infrastructure.Repositories.AuthRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.UserDto;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repository;
        private readonly ILogger<AuthController> _logger;
        public AuthController(ILogger<AuthController> logger, IAuthRepository repository)
        {
            _repository = repository;
            _logger = logger;

        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> LoginUser(PostUserDto postUserDto)
        {
            try
            {
                var (token, validationStatus) = await _repository.LoginUser(postUserDto.Username, postUserDto.Password);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error logging in User: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("User Not Found");
                if (validationStatus == ValidationStatus.Wrong_Password) return Unauthorized("Wrong Password");

                _logger.LogInformation($"User with username: {postUserDto.Username} logged in");
                return Ok(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to login User.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
