using AutoMapper;
using WhatsAppData.VO.WhatsApp;
using static WhatsAppData.VO.WhatsApp.MessageMediaVO;

namespace WhatsAppData.DTO.Chat;

public class ChatMessageMediaDTO : DTOBase
{
    public byte[] File { get; set; }
    public MediaTypes Type { get; set; }
}

public class ChatMessageMediaProfile : Profile
{
    public ChatMessageMediaProfile()
    {
        CreateMap<MessageMediaVO , ChatMessageMediaDTO>()
            .ForMember(dest => dest.Id , opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.File , opt => opt.MapFrom(src => src.File))
            .ForMember(dest => dest.Type , opt => opt.MapFrom(src => src.Type))
            ;
    }
}
