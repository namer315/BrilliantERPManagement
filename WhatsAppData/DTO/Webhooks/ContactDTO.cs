using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class ContactDTO
{
    public ProfileDTO? Profile { get; set; }

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class ProfileDTO
{
    public string Name { get; set; } = string.Empty;
}
