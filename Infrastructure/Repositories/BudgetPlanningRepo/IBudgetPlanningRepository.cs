using Entity;

namespace Infrastructure.Repositories.BudgetPlanningRepo
{
    public interface IBudgetPlanningRepository
    {
        Task<BudgetPlan?> CreateBudgetPlan(BudgetPlan budgetPlan, List<BudgetPlanItem> budgetPlanItems, int userId);
        Task<BudgetPlan?> GetBudgetPlan(int id);
        Task<BudgetPlan?> GetUserBudgetPlan(int userId);
    }
}