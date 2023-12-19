using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.ExpenseRepo
{
    public interface IExpenseRepository
    {
        Task<(Expense?, ValidationStatus)> AddExpense(Expense expense, int userId);
        Task<Expense?> GetExpense(int id);
        Task<List<Expense>?> GetUserExpenses(int userId);
        Task<(Expense?, ValidationStatus)> EditExpense(Expense expense, int id);
        Task<Expense?> DeleteExpense(int id);
    }
}
