using AutoMapper;
using RealTimeChat.Application.DTOs.Messages;
using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Application.Mappings.Message;

public class MessageMappingProfile : Profile
{
    public MessageMappingProfile()
    {
        // ChatMessage → MessageDto
        CreateMap<ChatMessage, MessageDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.FullName : string.Empty));

        // MessageReceipt → MessageReceiptDto
        CreateMap<MessageReceipt, MessageReceiptDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName ?? string.Empty));
    }
}
