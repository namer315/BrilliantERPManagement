namespace BrilliantWhatsAppAPI.DTO;

public class tTextMessageDTO
{
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
    public byte[] Photo { get; set; }
    public bool PreviewURL { get; set; } = true;

    public IList<tButtonDTO> ButtonList { get; set; }
}

public class tButtonDTO
{
    public ButtonType Type { get; set; } = ButtonType.Reply;
    public tReplyButtonDTO Reply { get; set; }

    public enum ButtonType
    {
        Reply
    }
}
public class tReplyButtonDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
}