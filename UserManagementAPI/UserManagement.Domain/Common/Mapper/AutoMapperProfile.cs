using AutoMapper;
using UserManagement.Domain.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Common.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpdateDto, ApplicationUser>().ReverseMap();
            CreateMap<RegisterDto, ApplicationUser>().ReverseMap();
            CreateMap<UserForAuthenticationDto, ApplicationUser>().ReverseMap();
        }
    }
}