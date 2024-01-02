namespace MoneyPlannerWebAPI.DTO.BudgetPlanningDto
{
    public class PostBudgetPlanningDto
    {
        public List<PostBudgetPlanningItemDto> BudgetPlanItemsDto { get; set; } = new();
        public double SummaryAmount { get; set; }
    }
}