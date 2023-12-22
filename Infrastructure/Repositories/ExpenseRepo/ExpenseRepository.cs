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

        public async Task<(Expense?, ValidationStatus)> EditExpense(Expense expense, int id)
        {
            var expenseFromDb = await GetExpense(id);

            if (expenseFromDb == null) return (null, ValidationStatus.Not_Found);
            if (!Validator.IsValidLength(expense.Title)) return (null, ValidationStatus.Invalid_Amount_Of_Characters);
            if (!Validator.IsValidAmount(expense.Amount)) return (null, ValidationStatus.Invalid_Amount);

            var newExpense = UpdateExpense(expenseFromDb, expense);

            await _context.SaveChangesAsync();
            return (newExpense, ValidationStatus.Success);
        }

        public async Task<Expense?> DeleteExpense(int id)
        {
            var expenseFromDb = await GetExpense(id);

            if (expenseFromDb == null) return null;

            _context.Expenses.Remove(expenseFromDb);
            await _context.SaveChangesAsync();

            return expenseFromDb;
        }

        public async Task<List<Expense>?> GetUserExpensesByMonth(int userId, int year, int monthNumber) =>
            await _context.Expenses.Where(x => x.User.Id == userId && x.Date.Year == year && x.Date.Month == monthNumber)
                                   .ToListAsync();


        #region Private Methods
        private Expense UpdateExpense(Expense expenseFromDb, Expense newExpense)
        {
            expenseFromDb.Title = newExpense.Title;
            expenseFromDb.ReOccuring = newExpense.ReOccuring;
            expenseFromDb.Date = newExpense.Date;
            expenseFromDb.Amount = newExpense.Amount;

            return newExpense;
        }
        
        #endregion
    }
}
