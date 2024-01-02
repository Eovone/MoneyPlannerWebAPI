namespace MoneyPlannerWebAPI.DTO.BudgetPlanningDto
{
    public class PostBudgetPlanningItemDto
    {
        public string Title { get; set; }
        public double Amount { get; set; }
        public bool IsIncome { get; set; }
    }
}