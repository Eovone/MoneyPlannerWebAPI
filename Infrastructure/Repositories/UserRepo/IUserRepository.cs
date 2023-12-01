using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<User> AddUser(string username, string password);
        Task<User> GetUser(int id);
        Task<LoginDto> LoginUser(string username, string password);
    }
}
