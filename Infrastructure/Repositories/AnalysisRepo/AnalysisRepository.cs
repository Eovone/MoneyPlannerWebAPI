using Entity;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.AnalysisRepo
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly DataContext _context;
        public AnalysisRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<(MonthAnalysis?, ValidationStatus)> CreateMonthAnalysis(int monthNumber, int year, int userId)
        {
            var user = await GetUserAsync(userId);
            if (user == null) return (null, ValidationStatus.Not_Found);

            var monthAnalysisList = await GetMonthAnalysis(monthNumber, year, user);
            if (monthAnalysisList.Count() != 0) await RemoveMonthAnalysis(monthAnalysisList);

            var incomes = await GetIncomes(monthNumber, year, userId);
            var expenses = await GetExpenses(monthNumber, year, userId);
            if (incomes.Count == 0 && expenses.Count == 0) return (null, ValidationStatus.No_Data_To_Make_Analysis);

            var summaryAmount = CalculateSummaryAmount(incomes, expenses);

            var monthAnalysis = PopulateMonthAnalysisProperties(monthNumber, year, incomes, expenses, summaryAmount);            
            
            user.MonthAnalysis.Add(monthAnalysis);
            await _context.SaveChangesAsync();
            return (monthAnalysis, ValidationStatus.Success);            
        }
        public async Task<MonthAnalysis?> GetMonthAnalysis(int id) => await _context.MonthAnalysis.FindAsync(id);

        public async Task<MonthAnalysis?> GetMonthAnalysisByMonth(int monthNumber, int year, int userId)
        {
            var user = await GetUserAsync(userId);
            if (user == null) return null;

            var monthAnalysis = await _context.MonthAnalysis.Where(x => x.User.Id == userId && x.Year == year && x.Month == monthNumber)
                                                            .Include(x => x.Incomes)
                                                            .Include(x => x.Expenses)
                                                            .FirstOrDefaultAsync();
                                                      
            if (monthAnalysis == null) return null;

            return monthAnalysis;
        }
       
        #region Private Methods
        private async Task<User?> GetUserAsync(int userId) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        private async Task<List<MonthAnalysis>> GetMonthAnalysis(int monthNumber, int year, User user)
        {
            var monthAnalysisList = await _context.MonthAnalysis.Where(x => x.User.Id == user.Id && x.Year == year && x.Month == monthNumber)
                                                                .Include(x => x.Incomes)
                                                                .Include(x => x.Expenses)
                                                                .ToListAsync();
            return monthAnalysisList;

        }
        private async Task RemoveMonthAnalysis(List<MonthAnalysis> listOfMonthAnalysis)
        {
            _context.MonthAnalysis.RemoveRange(listOfMonthAnalysis);
            await _context.SaveChangesAsync();
        }
        private double CalculateSummaryAmount(List<Income> incomes, List<Expense> expenses)
        {
            double summaryAmount = 0;
            foreach (var income in incomes)
            {
                summaryAmount += income.Amount;
            }
            foreach (var expense in expenses)
            {
                summaryAmount -= expense.Amount;
            }
            return summaryAmount;
        }
        private async Task<List<Income>> GetIncomes(int monthNumber, int year, int userId) => 
            await _context.Incomes.Where(x => x.User.Id == userId &&
                                              x.Date.Month == monthNumber &&
                                              x.Date.Year == year)
                                              .ToListAsync();
        private async Task<List<Expense>> GetExpenses(int monthNumber, int year, int userId) =>
            await _context.Expenses.Where(x => x.User.Id == userId &&
                                               x.Date.Month == monthNumber &&
                                               x.Date.Year == year)
                                               .ToListAsync();
        private MonthAnalysis PopulateMonthAnalysisProperties(int monthNumber, int year, List<Income> incomes, List<Expense> expenses, double summaryAmount)
        {
            var monthAnalysis = new MonthAnalysis();

            monthAnalysis.Incomes = incomes;
            monthAnalysis.Expenses = expenses;
            monthAnalysis.SummaryAmount = summaryAmount;
            monthAnalysis.Year = year;
            monthAnalysis.Month = monthNumber;

            return monthAnalysis;
        }
        #endregion

    }
}
