using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp;

public class MessageResponseDTO
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; }

    [JsonPropertyName("contacts")]
    public IList<ContactDTO> Contacts { get; set; }

    [JsonPropertyName("messages")]
    public IList<MessageDTO> Messages { get; set; }
}
