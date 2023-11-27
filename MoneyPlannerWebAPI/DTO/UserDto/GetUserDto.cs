using Entity;

namespace MoneyPlannerWebAPI.DTO.UserDto
{
    public class GetUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public List<Income> Incomes { get; set; }
        public List<Expense> Expenses { get; set; }
    }
}
