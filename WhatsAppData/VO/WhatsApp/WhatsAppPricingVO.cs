using CommonData.VO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class WhatsAppPricingVO : EntityBase
{
    public virtual bool? Billable { get; set; }          // <IS_BILLABLE?>
    public virtual string PricingModel { get; set; }    // <PRICING_MODEL>
    public virtual string Type { get; set; }            // <PRICING_TYPE>
    public virtual string Category { get; set; }        // <PRICING_CATEGORY>

    // Relationship back to MessageStatusVO
    public virtual MessageStatusVO MessageStatus { get; set; }
}

public class WhatsAppPricingMap : EntityWithCreatedAtMapping<WhatsAppPricingVO>
{
    public WhatsAppPricingMap()
    {
        Map(x => x.Billable).Nullable();
        Map(x => x.PricingModel);
        Map(x => x.Type);
        Map(x => x.Category);

        References(x => x.MessageStatus).Column("MessageStatus").Not.Nullable().Cascade.None();
    }
}