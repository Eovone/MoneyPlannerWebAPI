using MoneyPlannerWebAPI.DTO.ExpenseDto;
using MoneyPlannerWebAPI.DTO.IncomeDto;

namespace MoneyPlannerWebAPI.DTO.AnalysisDto
{
    public class GetMonthlyAnalysisDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<GetIncomeDto> Incomes { get; set; } = new();
        public List<GetExpenseDto> Expenses { get; set; } = new();
        public double SummaryAmount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
