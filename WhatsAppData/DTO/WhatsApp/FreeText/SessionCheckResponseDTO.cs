namespace WhatsAppData.DTO.WhatsApp.FreeText;

public class SessionCheckResponseDTO
{
    public string Phone { get; set; }
    public bool IsIn24hSession { get; set; }
    public DateTimeOffset? DateTime { get; set; }
}
