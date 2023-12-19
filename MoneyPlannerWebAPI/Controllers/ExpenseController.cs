﻿using AutoMapper;
using Entity;
using Infrastructure.Repositories.ExpenseRepo;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using MoneyPlannerWebAPI.DTO.ExpenseDto;

namespace MoneyPlannerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IExpenseRepository _repository;
        private readonly ILogger<ExpenseController> _logger;
        public ExpenseController(IMapper mapper, IExpenseRepository repository, ILogger<ExpenseController> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<GetExpenseDto>> CreateExpense(PostExpenseDto postExpenseDto, int userId)
        {
            try
            {
                var (createdExpense, validationStatus) = await _repository.AddExpense(_mapper.Map<Expense>(postExpenseDto), userId);

                if (validationStatus != ValidationStatus.Success) _logger.LogError("Error creating Expense: {ValidationStatus}", validationStatus.ToString());

                if (validationStatus == ValidationStatus.Not_Found) return NotFound("User Not Found");

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
                _logger.LogInformation($"Expense with Id: {expense.Id}, fetched successfully.");
                return Ok(_mapper.Map<GetExpenseDto>(expense));
            }
            catch (Exception e)
            {
                _logger.LogError("Error trying to get Expense with Id: {e}.", e);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}