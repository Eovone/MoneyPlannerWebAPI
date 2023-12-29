namespace Entity
{
    public class BudgetPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<BudgetPlanItem> BudgetPlanItems { get; set; } = new();
        public double SummaryAmount { get; set; }
    }
}