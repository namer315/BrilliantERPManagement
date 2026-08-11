using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class ErrorDTO
{
    public int Code { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    [JsonPropertyName("error_data")]
    public ErrorDataDTO? ErrorData { get; set; }

    public string? Href { get; set; }
}

public class ErrorDataDTO
{
    public string Details { get; set; } = string.Empty;
}
