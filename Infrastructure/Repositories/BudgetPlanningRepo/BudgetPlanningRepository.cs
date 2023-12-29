using Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.BudgetPlanningRepo
{
    public class BudgetPlanningRepository : IBudgetPlanningRepository
    {
        private readonly DataContext _context;
        public BudgetPlanningRepository(DataContext context)
        {
            _context = context;
        }
      

        public async Task<BudgetPlan?> CreateBudgetPlan(BudgetPlan budgetPlan, List<BudgetPlanItem> budgetPlanItems, int userId)
        {
            var previousBudgetPlan = await _context.BudgetPlans.Include(bp => bp.BudgetPlanItems)
                                                               .FirstOrDefaultAsync(bp => bp.UserId == userId);
            if (previousBudgetPlan != null)
            {
                _context.BudgetPlansItems.RemoveRange(previousBudgetPlan.BudgetPlanItems);
                _context.BudgetPlans.Remove(previousBudgetPlan);
                await _context.SaveChangesAsync();
            }            

            budgetPlan.UserId = userId;
            await _context.BudgetPlans.AddAsync(budgetPlan);

            foreach (var item in budgetPlanItems)
            {
                budgetPlan.BudgetPlanItems.Add(item);                
            }
            
            await _context.SaveChangesAsync();
            return budgetPlan;
        }

        public async Task<BudgetPlan?> GetBudgetPlan(int id)
        {
            return await _context.BudgetPlans.Include(bp => bp.BudgetPlanItems)
                                             .FirstOrDefaultAsync(bp => bp.Id == id);
        }

        public async Task<BudgetPlan?> GetUserBudgetPlan(int userId)
        {
            return await _context.BudgetPlans.Include(bp => bp.BudgetPlanItems)
                                             .FirstOrDefaultAsync(bp => bp.UserId == userId);
        }
    }
}