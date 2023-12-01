using AutoMapper;
using Entity;
using Infrastructure.Utilities;
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
            CreateMap<GetLoginUserDto, LoginDto>().ReverseMap();

            CreateMap<Income, PostIncomeDto>().ReverseMap();
            CreateMap<Income, GetIncomeDto>().ReverseMap();
        }
    }
}
