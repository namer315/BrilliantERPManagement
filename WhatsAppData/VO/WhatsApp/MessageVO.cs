using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class MessageVO : EntityBaseWithCode
{
    public virtual string MessageId { get; set; }     // WhatsApp message ID
    public virtual string Content { get; set; }       // Text or payload
    public virtual DateTime ReceivedAt { get; set; }  // Timestamp
    public virtual string Status { get; set; }        // delivered, read, etc.

    public virtual WhatsAppMessageTypes Type { get; set; }

    // Relationship
    public virtual ContactVO Sender { get; set; }
    public virtual ContactVO Receiver { get; set; }

    public virtual TenantVO Tenant { get; set; }


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
        System = 12
    }

}


public class MessageMap : EntityWithIdMapping<MessageVO>
{
    public MessageMap()
    {
        Map(x => x.MessageId).Not.Nullable();
        Map(x => x.Content).Length(int.MaxValue);//.Not.Nullable();
        //Map(x => x.ReceivedAt).Not.Nullable();
        Map(x => x.Status);
        Map(x => x.Type).Not.Nullable();

        References(x => x.Tenant).Column("Tenant").Not.Nullable().Cascade.None();
        References(x => x.Receiver).Column("Receiver").Not.Nullable().Cascade.Merge();
    }
}

