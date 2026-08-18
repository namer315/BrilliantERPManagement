namespace WhatsAppData.DTO.WhatsApp.FreeText;

public class SessionCheckResponseDTO
{
    public string Phone { get; set; }
    public bool IsIn24hSession { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public TimeSpan? TimeLeft => LastMessageAt.HasValue ? new TimeSpan(24,0,0) - (DateTimeOffset.UtcNow - LastMessageAt) : null;
}
