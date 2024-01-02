using AutoMapper;
using Entity;
using MoneyPlannerWebAPI.DTO.AnalysisDto;
using MoneyPlannerWebAPI.DTO.BudgetPlanningDto;
using MoneyPlannerWebAPI.DTO.ExpenseDto;
using MoneyPlannerWebAPI.DTO.IncomeDto;
using MoneyPlannerWebAPI.DTO.UserDto;

namespace MoneyPlannerWebAPI.Utilities
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, PostUserDto>().ReverseMap();
            CreateMap<User, GetUserDto>().ReverseMap();

            CreateMap<Income, PostIncomeDto>().ReverseMap();
            CreateMap<Income, GetIncomeDto>().ReverseMap();

            CreateMap<Expense, PostExpenseDto>().ReverseMap();
            CreateMap<Expense, GetExpenseDto>().ReverseMap();

            CreateMap<MonthAnalysis, GetMonthlyAnalysisDto>().ReverseMap();

            CreateMap<BudgetPlan, PostBudgetPlanningDto>().ReverseMap();
            CreateMap<BudgetPlan, GetBudgetPlanningDto>().ReverseMap();
            CreateMap<BudgetPlanItem, PostBudgetPlanningItemDto>().ReverseMap();
            CreateMap<BudgetPlanItem, GetBudgetPlanningItemDto>().ReverseMap();
        }
    }
}