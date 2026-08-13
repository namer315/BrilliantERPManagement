namespace WhatsAppData.DTO.WhatsApp.FreeText;

public class SessionCheckResponseDTO
{
    public string PhoneNumber { get; set; }
    public bool IsIn24hSession { get; set; }
    public TimeSpan? TimeLeft { get; set; }   // optional useful info
}
