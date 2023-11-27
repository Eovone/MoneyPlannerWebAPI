namespace Entity
{
    public class User
    {
        public User(string username)
        {
            Username = username;
        }
        public int Id { get; set; }
        public string Username { get; set; }
        public List<Income> Incomes { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
    }
}
