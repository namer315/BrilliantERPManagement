using CommonData.VO;

namespace WhatsAppData.VO.WhatsApp;

public class MessageButtonVO : EntityBase
{
    public virtual string Payload { get; set; }

    public virtual string Text { get; set; }

    public virtual MessageVO Message { get; set; }
}

public class MessageButtonMap : EntityWithIdMapping<MessageButtonVO>
{
    public MessageButtonMap()
    {
        Map(x => x.Payload);
        Map(x => x.Text);
    }
}