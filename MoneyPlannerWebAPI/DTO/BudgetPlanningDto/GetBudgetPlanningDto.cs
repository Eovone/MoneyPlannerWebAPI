namespace MoneyPlannerWebAPI.DTO.BudgetPlanningDto
{
    public class GetBudgetPlanningDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<GetBudgetPlanningItemDto> BudgetPlanItemsDto { get; set; } = new();
        public double SummaryAmount { get; set; }
    }
}