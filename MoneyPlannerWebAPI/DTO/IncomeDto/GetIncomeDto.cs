namespace MoneyPlannerWebAPI.DTO.IncomeDto
{
    public class GetIncomeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public bool ReOccuring { get; set; }
    }
}
