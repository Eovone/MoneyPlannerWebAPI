using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.BudgetPlanningRepo
{
    public interface IBudgetPlanningRepository
    {
        Task<(BudgetPlan?, ValidationStatus)> CreateBudgetPlan(BudgetPlan budgetPlan, List<BudgetPlanItem> budgetPlanItems, int userId);
        Task<BudgetPlan?> GetBudgetPlan(int id);
        Task<BudgetPlan?> GetUserBudgetPlan(int userId);
    }
}