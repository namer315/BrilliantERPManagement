using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class MessageMediaVO : EntityBase
{
    public virtual byte[] MediaFile { get; set; }
    public virtual string FileName { get; set; }
    public virtual MediaTypes Type { get; set; }


    public virtual MessageVO Message { get; set; }

    public enum MediaTypes
    {
        Image = 1,
        Video = 2,
        Document = 3,
        Audio = 4
    }
}
public class MessageMediaMap : EntityWithIdMapping<MessageMediaVO>
{
    public MessageMediaMap()
    {
        Map(x => x.MediaFile).Length(int.MaxValue).Not.Nullable();
        Map(x => x.FileName);
        Map(x => x.Type).Not.Nullable();

        References(x => x.Message).Column("Message").Not.Nullable().Cascade.None();
    }
}
