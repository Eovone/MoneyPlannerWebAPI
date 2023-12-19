namespace MoneyPlannerWebAPI.DTO.ExpenseDto
{
    public class GetExpenseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public bool ReOccuring { get; set; }
    }
}
