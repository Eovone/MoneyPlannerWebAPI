using Entity;

namespace Infrastructure.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<User> AddUser(string username, string password);
        Task<User> GetUser(int id);
        Task<bool> LoginUser(string username, string password);
    }
}
