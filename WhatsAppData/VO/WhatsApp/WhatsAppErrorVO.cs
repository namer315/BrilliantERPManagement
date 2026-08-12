using CommonData.VO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class WhatsAppErrorVO : EntityBase
{
    public virtual int ErrorCode { get; set; }          // <ERROR_CODE>
    public virtual string Title { get; set; }      // <ERROR_TITLE>
    public virtual string Message { get; set; }    // <ERROR_MESSAGE>
    public virtual string Details { get; set; }    // <ERROR_DETAILS>
    public virtual string Href { get; set; }       // <ERROR_CODES_URL> https://developers.facebook.com/docs/whatsapp/cloud-api/support/error-codes/

    // Relationship back to MessageStatusVO (optional, if you want to link errors to statuses)
    public virtual MessageStatusVO MessageStatus { get; set; }
}
public class WhatsAppErrorMap : EntityWithCreatedAtMapping<WhatsAppErrorVO>
{
    public WhatsAppErrorMap()
    {
        Map(x => x.ErrorCode).Not.Nullable();
        Map(x => x.Title);
        Map(x => x.Message);
        Map(x => x.Details).Length(int.MaxValue);
        //Map(x => x.Href).Length(int.MaxValue);

        References(x => x.MessageStatus).Column("MessageStatus").Cascade.None()/*.Nullable()*/;
    }
}