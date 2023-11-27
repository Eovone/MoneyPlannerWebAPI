using Entity;

namespace Infrastructure.Repositories.IncomeRepo
{
    public class IncomeRepository : IIncomeRepository
    {
        private readonly DataContext _context;
        public IncomeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Income> AddIncome(Income income, int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return null;

            user.Incomes.Add(income);
            await _context.SaveChangesAsync();

            return income;
        }

        public async Task<Income> GetIncome(int id)
        {
            var income = _context.Incomes.FindAsync(id);

            if (income.Result == null) return null;

            return income.Result;
        }

        public async Task<List<Income>> GetUserIncomes(int userId)
        {
            var incomeList = _context.Incomes.Where(x => x.User.Id == userId).ToList();

            if (incomeList.Count == 0) return null;

            return incomeList;
        }
    }
}
