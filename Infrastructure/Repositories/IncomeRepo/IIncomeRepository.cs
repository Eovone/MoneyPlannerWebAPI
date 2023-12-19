using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.IncomeRepo
{
    public interface IIncomeRepository
    {
        Task<(Income?, ValidationStatus)> AddIncome(Income income, int userId);
        Task<Income?> GetIncome(int id);
        Task<List<Income>?> GetUserIncomes(int userId);
        Task<(Income?, ValidationStatus)> EditIncome(Income income, int id);
        Task<Income?> DeleteIncome(int id);
    }
}
