using Entity;
using Infrastructure.Utilities;

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

            //Todo: validate the expense and return proper validationstatus

            user.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return (expense, ValidationStatus.Success);
        }

        public async Task<Expense?> GetExpense(int id) => await _context.Expenses.FindAsync(id);        
    }
}
