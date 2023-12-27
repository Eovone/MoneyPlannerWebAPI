using Infrastructure.Utilities;

namespace Infrastructure.Repositories.AuthRepo
{
    public interface IAuthRepository
    {
        Task<(string?, ValidationStatus)> LoginUser(string username, string password);
    }
}
