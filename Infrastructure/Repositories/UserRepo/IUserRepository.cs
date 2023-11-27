using Entity;

namespace Infrastructure.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<User> AddUser(User user);
        Task<User> GetUser(int id);
    }
}
