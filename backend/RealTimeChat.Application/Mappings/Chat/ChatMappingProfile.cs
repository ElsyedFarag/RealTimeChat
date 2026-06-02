using AutoMapper;
using RealTimeChat.Application.DTOs.Chats;
using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Application.Mappings.Chat;

public class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        // Chat → ChatDto
        CreateMap<RealTimeChat.Domain.Entities.Chat, ChatDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        // Chat → ChatDetailsDto
        // ParticipantsCount and Participants mapped from nav prop
        CreateMap<RealTimeChat.Domain.Entities.Chat, ChatDetailsDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.ParticipantsCount, opt => opt.MapFrom(src => src.Participants.Count))
            .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants));

        // ChatParticipant → ChatParticipantDto
        // ProfilePictureUrl URL resolution is done in service via IUrlBuilder
        CreateMap<ChatParticipant, ChatParticipantDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName ?? string.Empty))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.User.IsOnline));
    }
}
