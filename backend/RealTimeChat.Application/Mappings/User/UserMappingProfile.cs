using AutoMapper;
using RealTimeChat.Application.DTOs.Users;
using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Application.Mappings.User;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        
        CreateMap<AppUser, AppUserDto>()
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));
    }
}

