using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.Webhooks;

internal class WhatsAppErrorBE
{
    internal WhatsAppErrorVO GetNew(MessageStatusVO messageStatus)
    {
        if (messageStatus is null)
            throw new ArgumentNullException(nameof(messageStatus));

        WhatsAppErrorVO whatsAppError = new WhatsAppErrorVO();
        whatsAppError.MessageStatus = messageStatus;

        return whatsAppError;
    }
}
