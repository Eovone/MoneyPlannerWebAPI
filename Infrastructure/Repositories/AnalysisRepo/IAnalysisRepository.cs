using Entity;

namespace Infrastructure.Repositories.AnalysisRepo
{
    public interface IAnalysisRepository
    {
        Task<MonthAnalysis?> CreateMonthAnalysis(int monthNumber, int year, int userId);
    }
}
