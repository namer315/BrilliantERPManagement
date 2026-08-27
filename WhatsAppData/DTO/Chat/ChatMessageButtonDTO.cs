using AutoMapper;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DTO.Chat;

public class ChatMessageButtonDTO : DTOBase
{
    public string Payload { get; set; }
    public string Text { get; set; }
}
public class ChatMessageButtonProfile : Profile
{
    public ChatMessageButtonProfile()
    {
        CreateMap<MessageButtonVO , ChatMessageButtonDTO>()
            .ForMember(dest => dest.Id , opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Text , opt => opt.MapFrom(src => src.Text))
            .ForMember(dest => dest.Payload , opt => opt.MapFrom(src => src.Payload))
            ;
    }
}
