namespace MoneyPlannerWebAPI.DTO.ExpenseDto
{
    public class PostExpenseDto
    {
        public PostExpenseDto(string title, double amount, DateTime date)
        {
            Title = title;
            Amount = amount;
            Date = date;
        }
        public string Title { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public bool ReOccuring { get; set; }
    }
}
