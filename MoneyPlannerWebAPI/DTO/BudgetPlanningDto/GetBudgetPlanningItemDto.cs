namespace MoneyPlannerWebAPI.DTO.BudgetPlanningDto
{
    public class GetBudgetPlanningItemDto
    {
        public string Title { get; set; }
        public double Amount { get; set; }
        public bool IsIncome { get; set; }
    }
}