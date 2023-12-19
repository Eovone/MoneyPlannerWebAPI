using Entity;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.ExpenseRepo
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly DataContext _context;
        public ExpenseRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<(Expense?, ValidationStatus)> AddExpense(Expense expense, int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null) return (null, ValidationStatus.Not_Found);
            if (!Validator.IsValidLength(expense.Title)) return (null, ValidationStatus.Invalid_Amount_Of_Characters);
            if (!Validator.IsValidAmount(expense.Amount)) return (null, ValidationStatus.Invalid_Amount);

            user.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return (expense, ValidationStatus.Success);
        }

        public async Task<Expense?> GetExpense(int id) => await _context.Expenses.FindAsync(id);

        public async Task<List<Expense>?> GetUserExpenses(int userId) => await _context.Expenses.Where(x => x.User.Id == userId).ToListAsync();
      
    }
}
