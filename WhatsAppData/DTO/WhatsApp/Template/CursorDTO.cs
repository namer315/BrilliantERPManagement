namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// Cursor pointers for navigating paginated results.
/// </summary>
public class CursorDTO
{
    public string? Before { get; set; }

    public string? After { get; set; }
}
