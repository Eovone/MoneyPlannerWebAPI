namespace MoneyPlannerWebAPI.DTO.IncomeDto
{
    public class PostIncomeDto
    {
        public PostIncomeDto(string title, double amount, DateTime date)
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
