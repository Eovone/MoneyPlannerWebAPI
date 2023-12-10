using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<(User?, ValidationStatus)> AddUser(string username, string password);
        Task<User?> GetUser(int id);
        Task<(LoginDto?, ValidationStatus)> LoginUser(string username, string password);
    }
}
