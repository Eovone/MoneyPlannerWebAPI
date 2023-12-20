using Entity;
using Infrastructure.Utilities;

namespace Infrastructure.Repositories.AnalysisRepo
{
    public interface IAnalysisRepository
    {
        Task<(MonthAnalysis?, ValidationStatus)> CreateMonthAnalysis(int monthNumber, int year, int userId);
        Task<MonthAnalysis?> GetMonthAnalysis(int id);
    }
}
