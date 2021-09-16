using AutoMapper;
using Identity.Models;
using Identity.Models.InputModels;

namespace Identity.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCustomer, AppUser>()
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.PasswordHash, opt => opt.MapFrom(s => s.Password))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(s => s.LastName))
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.UserName));
        }
    }
}
