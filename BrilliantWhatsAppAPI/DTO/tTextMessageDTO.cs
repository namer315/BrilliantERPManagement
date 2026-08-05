namespace BrilliantWhatsAppAPI.DTO;

public class tTextMessageDTO
{
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
    public byte[] Photo { get; set; }
    public byte[] Video { get; set; }
    public byte[] Audio { get; set; }
    public byte[] Document { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public string Footer { get; set; }   // interactive messages only (max 60 chars)
    public bool PreviewURL { get; set; } = true;

    // --- Interactive message fields ---
    // Which interactive layout to build; null/empty falls back to the legacy button layout.
    public string InteractiveType { get; set; }   // "button" | "list" | "cta_url" | "location"
    public string HeaderText { get; set; }        // interactive text header (max 60 chars)

    // List messages
    public string ListButtonText { get; set; }    // list action button label (max 20 chars)
    public IList<tListSectionDTO> ListSections { get; set; }

    // CTA URL messages
    public string CtaDisplayText { get; set; }    // button label (max 20 chars)
    public string CtaUrl { get; set; }            // URL the button opens

    public IList<tButtonDTO> ButtonList { get; set; }
}

public class tListSectionDTO
{
    public string Title { get; set; }             // section title (max 24 chars)
    public IList<tListRowDTO> Rows { get; set; }
}

public class tListRowDTO
{
    public string Id { get; set; }                // row id (max 200 chars)
    public string Title { get; set; }             // row title (max 24 chars)
    public string Description { get; set; }       // optional (max 72 chars)
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