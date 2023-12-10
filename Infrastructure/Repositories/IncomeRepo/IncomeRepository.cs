using Entity;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.IncomeRepo
{
    public class IncomeRepository : IIncomeRepository
    {
        private readonly DataContext _context;
        public IncomeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<(Income?, ValidationStatus)> AddIncome(Income income, int userId)
        {  
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return (null, ValidationStatus.Not_Found);

            //Todo: validate the income and return proper validationstatuses

            user.Incomes.Add(income);
            await _context.SaveChangesAsync();

            return (income, ValidationStatus.Success);
        }

        public async Task<Income?> GetIncome(int id) => await _context.Incomes.FindAsync(id);      

        public async Task<List<Income>?> GetUserIncomes(int userId)
        {
            var incomeList = await _context.Incomes.Where(x => x.User.Id == userId).ToListAsync();

            if (incomeList.Count == 0) return null;

            return incomeList;
        }

    }
}
