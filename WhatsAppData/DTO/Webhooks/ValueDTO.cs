using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class ValueDTO
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    public MetadataDTO Metadata { get; set; } = new();

    public IList<ContactDTO>? Contacts { get; set; }

    public IList<MessageDTO>? Messages { get; set; }

    public IList<StatusDTO>? Statuses { get; set; }
}
