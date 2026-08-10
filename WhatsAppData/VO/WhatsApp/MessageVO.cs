using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class MessageVO : AppBaseEntityWithCode
{
    public virtual string MessageId { get; set; }     // WhatsApp message ID
    public virtual string Content { get; set; }       // Text or payload
    public virtual DateTime ReceivedAt { get; set; }  // Timestamp
    public virtual string Status { get; set; }        // delivered, read, etc.

    // Relationship
    public virtual ContactVO Sender { get; set; }
    public virtual ContactVO Receiver { get; set; }


    public enum WhatsAppMessageType
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

