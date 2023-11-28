using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> AddUser(string username, string password)
        {
            if (PasswordValidator.Validate(password) == false) return null;

            if (IsUsernameTaken(username)) return null;

            var passwordSalt = PasswordHasher.GenerateSalt();
            var passwordHash = PasswordHasher.HashPassword(password, passwordSalt);

            var user = new User(username, passwordSalt, passwordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return null;

            return user;
        }

        #region Private Methods
        private bool IsUsernameTaken(string username)
        {
            var foundUsername = _context.Users.Where(u => u.Username == username).Any();
            if (foundUsername == true) return true;
            return false;
        }
        #endregion
    }
}
