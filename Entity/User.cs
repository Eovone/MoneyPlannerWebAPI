
namespace Entity
{
    public class User
    {
        private User() { }
        public User(string username, byte[] passwordsalt, string passwordhash)
        {
            Username = username;
            PasswordSalt = passwordsalt;
            PasswordHash = passwordhash;
        }
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordSalt { get; private set; }
        public string PasswordHash { get; private set; }
        public List<Income> Incomes { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
    }
}
