using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.ExpenseRepo
{
    public interface IExpenseRepository
    {
        Task<(Expense?, ValidationStatus)> AddExpense(Expense expense, int userId);
        Task<Expense?> GetExpense(int id);
    }
}
