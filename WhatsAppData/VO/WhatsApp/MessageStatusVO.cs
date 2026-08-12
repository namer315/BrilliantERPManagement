using CommonData.VO;
using FluentNHibernate.Mapping;

namespace WhatsAppData.VO.WhatsApp;

public class MessageStatusVO : EntityBase
{
    public virtual WhatsAppMessageStatus Status { get; set; }   // e.g. "accepted", "delivered"
    public virtual long Timestamp { get; set; }        // raw webhook timestamp (Unix seconds)

    // Relationship back to Message
    public virtual MessageVO Message { get; set; }

    #region enum
    public enum WhatsAppMessageStatus
    {
        Sent = 1,        // One checkmark in WhatsApp UI
        Delivered = 2,   // Two gray checkmarks
        Read = 3,        // Two blue checkmarks
        Failed = 4,      // Red error triangle
        Played = 5       // Blue microphone (voice message played)
    }
    #endregion
}
public class MessageStatusMap : EntityWithCreatedAtMapping<MessageStatusVO>
{
    public MessageStatusMap()
    {

        Map(x => x.Status).Not.Nullable();
        Map(x => x.Timestamp).Not.Nullable();

        References(x => x.Message).Column("MessageId").Not.Nullable().Cascade.None();
    }
}