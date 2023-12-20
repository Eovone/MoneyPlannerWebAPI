using Entity;
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

        public async Task<MonthAnalysis?> CreateMonthAnalysis(int monthNumber, int year, int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            // check if there is a monthAnalysis previously for this month
            // if it exists, remove it before creating the new one

            var monthAnalysis = new MonthAnalysis();   

            var incomes = await _context.Incomes.Where(x => x.User.Id == userId && 
                                                            x.Date.Month == monthNumber &&
                                                            x.Date.Year == year)
                                                            .ToListAsync();
            monthAnalysis.Incomes = incomes;

            var expenses = await _context.Expenses.Where(x => x.User.Id == userId &&
                                                            x.Date.Month == monthNumber &&
                                                            x.Date.Year == year)
                                                            .ToListAsync();
            monthAnalysis.Expenses = expenses;

            double summaryAmount = 0;
            foreach (var income in incomes)
            {
                summaryAmount += income.Amount;
            }
            foreach (var expense in expenses)
            {
                summaryAmount -= expense.Amount;
            }
            monthAnalysis.SummaryAmount = summaryAmount;
            monthAnalysis.Year = year;
            monthAnalysis.Month = monthNumber;
            
            user.MonthAnalysis.Add(monthAnalysis);
            await _context.SaveChangesAsync();

            return monthAnalysis;            
        }
    }
}
