namespace WhatsAppData.DTO.Webhooks;

public class ChangeDTO
{
    public ValueDTO Value { get; set; } = new();

    public string Field { get; set; } = string.Empty;
}
