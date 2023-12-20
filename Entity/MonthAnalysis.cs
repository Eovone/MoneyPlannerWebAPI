namespace Entity
{
    public class MonthAnalysis
    {
        public int Id { get; set; }
        public User User { get; set; }
        public ICollection<Income>? Incomes { get; set; }
        public ICollection<Expense>? Expenses { get; set; }
        public double SummaryAmount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

    }
}
