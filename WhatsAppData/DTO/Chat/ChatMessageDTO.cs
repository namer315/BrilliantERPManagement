using AutoMapper;
using System.Text.Json.Serialization;
using WhatsAppData.DTO.Common;
using WhatsAppData.VO.WhatsApp;
using static WhatsAppData.VO.WhatsApp.MessageStatusVO;
using static WhatsAppData.VO.WhatsApp.MessageVO;

namespace WhatsAppData.DTO.Chat;

public class ChatMessageDTO : DTOBase
{
    public string MessageId { get; set; }

    public WhatsAppMessageTypes Type { get; set; }

    [JsonIgnore]
    public long? Timestamp { get; set; }
    public DateTimeOffset? DateTimeUTC => Timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(Timestamp.Value) : null;

    //public bool HasPreviewUrl { get; init; } = false;

    public MessageDirections MessageDirection { get; set; }

    public WhatsAppMessageStatus? Status { get; set; }

    public string Body { get; set; }


    public ContactDTO Contact { get; set; }

    #region Releted
    public ChatMessageMediaDTO Media { get; set; }
    public ChatMessageButtonDTO Button { get; set; }
    #endregion

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageDirections
    {
        Incoming,
        Outgoing
    }
}
public class ChatMessageProfile : Profile
{
    public ChatMessageProfile()
    {
        CreateMap<MessageVO , ChatMessageDTO>()
            .ForMember(dest => dest.Id , opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MessageId , opt => opt.MapFrom(src => src.MessageId))
            .ForMember(dest => dest.MessageDirection , opt => opt.MapFrom(src => src.MessageDirection))
            .ForMember(dest => dest.Body , opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Status , opt => opt.Ignore())

            .ForMember(dest => dest.Contact , opt => opt.MapFrom(src => src.Media))
            .ForMember(dest => dest.Media , opt => opt.MapFrom(src => src.Media))
            .ForMember(dest => dest.Button , opt => opt.MapFrom(src => src.Button))
            ;
    }
}

