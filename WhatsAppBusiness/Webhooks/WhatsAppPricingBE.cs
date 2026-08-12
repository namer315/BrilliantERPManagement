using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.Webhooks;

internal class WhatsAppPricingBE
{
    WhatsAppPricingDAO _dao = new WhatsAppPricingDAO();

    internal WhatsAppPricingVO GetNew(MessageStatusVO messageStatus)
    {
        if (messageStatus is null)
            throw new ArgumentNullException(nameof(messageStatus));

        WhatsAppPricingVO pricing = new WhatsAppPricingVO();
        pricing.MessageStatus = messageStatus;

        return pricing;
    }

    internal async Task<string> Persist(WhatsAppPricingVO pricing)
    {
        Validation(pricing);

        return await _dao.PersistAsync(pricing);
    }

    private void Validation(WhatsAppPricingVO pricing)
    {

    }
}
