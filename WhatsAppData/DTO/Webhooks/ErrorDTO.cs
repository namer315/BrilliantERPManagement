using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

// Wrapper class to match the root {"error": { ... }} object
public class ErrorResponseDTO
{
    public ErrorDTO Error { get; set; }
}
public class ErrorDTO
{
    public int Code { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    [JsonPropertyName("error_data")]
    public ErrorDataDTO? ErrorData { get; set; }

    [JsonPropertyName("fbtrace_id")]
    public string FbTraceId { get; set; }

    public string Type { get; set; }

    public string? Href { get; set; }
}

public class ErrorDataDTO
{
    public string Details { get; set; } = string.Empty;
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;
}