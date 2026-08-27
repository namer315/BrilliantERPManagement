using CommonData.VO;
using static WhatsAppData.DTO.Chat.ChatMessageDTO;

namespace WhatsAppData.VO.WhatsApp;

public class MessageVO : EntityBaseWithCode
{
    public virtual string MessageId { get; set; }     // WhatsApp message ID
    public virtual string Content { get; set; }       // Text or payload
    public virtual DateTime ReceivedAt { get; set; }  // Timestamp
    public virtual string Status { get; set; }        // delivered, read, etc.
    public virtual long? Timestamp { get; set; } = null;       // raw webhook timestamp (Unix seconds)

    public virtual WhatsAppMessageTypes Type { get; set; }

    public virtual MessageDirections MessageDirection => Receiver is null ? MessageDirections.Incoming : MessageDirections.Outgoing;

    // Relationship
    public virtual ContactVO Sender { get; set; }
    public virtual ContactVO Receiver { get; set; }

    public virtual TenantVO Tenant { get; set; }

    public virtual MessageMediaVO Media { get; set; }
    //Types Relation
    public virtual MessageButtonVO Button { get; set; }

    // Status history (delivered, read, ...) — inverse of MessageStatusVO.Message
    public virtual IList<MessageStatusVO> StatuseList { get; set; } = new List<MessageStatusVO>();


    public enum WhatsAppMessageTypes
    {
        Text = 1,
        Template = 2,
        Image = 3,
        Video = 4,
        Audio = 5,
        Document = 6,
        Sticker = 7,
        Location = 8,
        Contacts = 9,
        Interactive = 10,
        Reaction = 11,
        System = 12,
        Button = 13,
    }

}


public class MessageMap : EntityWithDatesMapping<MessageVO>
{
    public MessageMap()
    {
        Map(x => x.MessageId)/*.Not.Nullable()*/;
        Map(x => x.Content).Length(int.MaxValue);//.Not.Nullable();
        //Map(x => x.ReceivedAt).Not.Nullable();
        Map(x => x.Status);
        Map(x => x.Timestamp).Nullable();
        Map(x => x.Type).Not.Nullable();

        References(x => x.Tenant).Column("Tenant")/*.Not.Nullable()*/.Cascade.None();
        References(x => x.Receiver).Column("Receiver")/*.Not.Nullable()*/.Cascade.Merge();
        References(x => x.Sender).Column("Sender")/*.Not.Nullable()*/.Cascade.Merge();

        References(x => x.Button , "Button").Cascade.Merge();
        References(x => x.Media , "Media").Cascade.Merge();


        HasMany(x => x.StatuseList).KeyColumn("Message")   // matches MessageStatusMap: References(x => x.Message).Column("Message")
           .Inverse()
           .Cascade.All();
    }
}

