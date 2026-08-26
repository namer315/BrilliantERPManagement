namespace WhatsAppData.DTO.WhatsApp.FreeText;

public class TextDTO
{
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
    public bool PreviewURL { get; set; } = true;
}
