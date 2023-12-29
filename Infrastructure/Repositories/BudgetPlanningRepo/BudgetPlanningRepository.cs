using Entity;
using Infrastructure.Utilities;
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
      

        public async Task<(BudgetPlan?, ValidationStatus)> CreateBudgetPlan(BudgetPlan budgetPlan, List<BudgetPlanItem> budgetPlanItems, int userId)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return (null, ValidationStatus.Not_Found);

            foreach (var item in budgetPlanItems)
            {
                if (!Validator.IsValidLength(item.Title)) return (null, ValidationStatus.Invalid_Amount_Of_Characters);
                if (!Validator.IsValidAmount(item.Amount)) return (null, ValidationStatus.Invalid_Amount);
            }

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
            return (budgetPlan, ValidationStatus.Success);
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