using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class MetadataDTO
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}
