using System.Collections.Generic;

namespace WhatsAppData.DTO.Webhooks;

public class EntryDTO
{
    public string Id { get; set; } = string.Empty;

    // Unix epoch seconds
    public long Time { get; set; }

    public IList<ChangeDTO> Changes { get; set; } = new List<ChangeDTO>();
}
