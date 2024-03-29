﻿using AutoMapper;
using Entity;
using Infrastructure.Repositories.ExpenseRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.ExpenseDto;
using MoneyPlannerWebAPI.Utilities;
using System.Security.Claims;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IExpenseRepository _repository;
        private readonly ILogger<ExpenseController> _logger;
        private readonly IAuthorizationHelper _authorizationHelper;
        public ExpenseController(IMapper mapper, IExpenseRepository repository, ILogger<ExpenseController> logger, IAuthorizationHelper authorizationHelper)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _authorizationHelper = authorizationHelper;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetExpenseDto>> CreateExpense(PostExpenseDto postExpenseDto, int userId)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var (createdExpense, validationStatus) = await _repository.AddExpense(_mapper.Map<Expense>(postExpenseDto), userId);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating Expense: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("User Not Found");
                if (validationStatus == ValidationStatus.Invalid_Amount_Of_Characters) return BadRequest($"{validationStatus} in the title.");
                if (validationStatus == ValidationStatus.Invalid_Amount) return BadRequest($"{validationStatus}");

                var getExpenseDto = _mapper.Map<GetExpenseDto>(createdExpense);
                _logger.LogInformation($"Expense with Id: {getExpenseDto.Id} was successfully created.");
                return CreatedAtAction("GetExpense", new { id = getExpenseDto.Id }, getExpenseDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to create Expense.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetExpenseDto>> GetExpense(int id)
        {
            try
            {
                var expense = await _repository.GetExpense(id);
                if (expense == null)
                {
                    _logger.LogError($"Expense with Id: {id}, does not exist.");
                    return NotFound($"Expense with Id: {id}, could not be found.");
                }

                if (!_authorizationHelper.IsUserAuthorized(User, expense.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${expense.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                _logger.LogInformation($"Expense with Id: {expense.Id}, fetched successfully.");
                return Ok(_mapper.Map<GetExpenseDto>(expense));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Expense with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("User/{userId}")]
        public async Task<ActionResult<List<GetExpenseDto>>> GetUserExpenses(int userId)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var expenseList = await _repository.GetUserExpenses(userId);

                _logger.LogInformation($"Expenses for user with Id: {userId}, fetched successfully.");
                return Ok(_mapper.Map<List<GetExpenseDto>>(expenseList));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Expenses for user with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GetExpenseDto>> EditExpense(PostExpenseDto postExpense, int id)
        {
            try
            {
                var originalExpense = await _repository.GetExpense(id);
                if (originalExpense == null) return NotFound("Expense not found");
                if (!_authorizationHelper.IsUserAuthorized(User, originalExpense.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${originalExpense.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var (editedExpense, validationStatus) = await _repository.EditExpense(_mapper.Map<Expense>(postExpense), id);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating Expense: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("Expense not found");
                if (validationStatus == ValidationStatus.Invalid_Amount_Of_Characters) return BadRequest($"{validationStatus} in the title.");
                if (validationStatus == ValidationStatus.Invalid_Amount) return BadRequest($"{validationStatus}");

                _logger.LogInformation($"Expense with Id: {id}, edited successfully.");
                return Ok(_mapper.Map<GetExpenseDto>(editedExpense));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to edit Expense: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteExpense(int id)
        {
            try
            {
                var originalExpense = await _repository.GetExpense(id);
                if (originalExpense == null) return NotFound("Income not found");
                if (!_authorizationHelper.IsUserAuthorized(User, originalExpense.User.Id))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${originalExpense.User.Id}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var deletedExpense = await _repository.DeleteExpense(id);
                if (deletedExpense == null)
                {
                    _logger.LogError($"Expense with Id: {id}, does not exist.");
                    return NotFound($"Expense with Id: {id}, could not be found.");
                }
                _logger.LogInformation($"Expense with Id: {id}, deleted successfully.");
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to delete Expense: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("User/{userId}/Year/{year}/Month/{monthNumber}")]
        public async Task<ActionResult<List<GetExpenseDto>>> GetUserExpensesByMonth(int userId, int year, int monthNumber)
        {
            try
            {
                if (!_authorizationHelper.IsUserAuthorized(User, userId))
                {
                    _logger.LogWarning($"User with Id: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value!} was denied access to controller for User with Id: ${userId}.");
                    return Unauthorized("You are not authorized to perform this action.");
                }

                var expenseList = await _repository.GetUserExpensesByMonth(userId, year, monthNumber);

                _logger.LogInformation($"Expenses for user with Id: {userId}, fetched successfully.");
                return Ok(_mapper.Map<List<GetExpenseDto>>(expenseList));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Expenses for user with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
