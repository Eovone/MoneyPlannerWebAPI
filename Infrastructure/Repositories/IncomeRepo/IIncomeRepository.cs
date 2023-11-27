using Entity;

namespace Infrastructure.Repositories.IncomeRepo
{
    public interface IIncomeRepository
    {
        Task<Income> AddIncome(Income income, int userId);
        Task<Income> GetIncome(int id);
        Task<List<Income>> GetUserIncomes(int userId);
    }
}
