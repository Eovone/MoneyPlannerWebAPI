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

        public async Task<List<Income>?> GetUserIncomes(int userId) => await _context.Incomes.Where(x => x.User.Id == userId).ToListAsync();

        public async Task<Income?> EditIncome(Income income, int id)
        {
            var incomeFromDb = await GetIncome(id);

            if (incomeFromDb == null) return null;

            var newIncome = UpdateIncome(incomeFromDb, income);       

            await _context.SaveChangesAsync();
            return newIncome;
        }

        public async Task<Income?> DeleteIncome(int id)
        {
            var incomeFromDb = await GetIncome(id);

            if (incomeFromDb == null) return null;

            _context.Incomes.Remove(incomeFromDb);
            await _context.SaveChangesAsync();

            return incomeFromDb;
        }

        #region Private Methods
        public Income UpdateIncome(Income incomeFromDb, Income newIncome)
        {
            incomeFromDb.Title = newIncome.Title;
            incomeFromDb.ReOccuring = newIncome.ReOccuring;
            incomeFromDb.Date = newIncome.Date;
            incomeFromDb.Amount = newIncome.Amount;
            
            return incomeFromDb;
        }

       
        #endregion
    }
}
