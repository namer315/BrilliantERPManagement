using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Common;

public class ContactDTO
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string WaId { get; set; }
}
