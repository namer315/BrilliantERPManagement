using AutoMapper;
using System.Text.Json.Serialization;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DTO.Common;

public class ContactDTO
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string WaId { get; set; }
}
public class ContactProfile : Profile
{
    public ContactProfile()
    {
        CreateMap<ContactVO , ContactDTO>()
            .ForMember(dest => dest.Id , opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.WaId , opt => opt.MapFrom(src => src.WaId))
            ;
    }
}