using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp;

public class MessageResponseDTO
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; }

    public IList<ContactDTO> Contacts { get; set; }

    public IList<MessageDTO> Messages { get; set; }
}
