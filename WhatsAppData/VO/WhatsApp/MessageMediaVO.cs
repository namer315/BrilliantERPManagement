using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class MessageMediaVO : EntityBase
{
    public virtual byte[] File { get; set; }

    public virtual MediaTypes Type { get; set; }


    public virtual MessageVO Message { get; set; }

    public enum MediaTypes
    {
        Document
    }
}
public class MessageMediaMap : EntityWithCreatedAtMapping<MessageMediaVO>
{
    public MessageMediaMap()
    {
        Map(x => x.File).Not.Nullable();
        Map(x => x.Type).Not.Nullable();

        References(x => x.Message).Column("Message").Not.Nullable().Cascade.None();
    }
}
