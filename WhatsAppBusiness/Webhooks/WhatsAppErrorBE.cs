using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.Webhooks;

internal class WhatsAppErrorBE
{
    WhatsAppErrorDAO _dao = new WhatsAppErrorDAO();
    internal WhatsAppErrorVO GetNew(MessageStatusVO messageStatus)
    {
        if (messageStatus is null)
            throw new ArgumentNullException(nameof(messageStatus));

        WhatsAppErrorVO whatsAppError = new WhatsAppErrorVO();
        whatsAppError.MessageStatus = messageStatus;

        return whatsAppError;
    }

    internal async Task<string> Persist(WhatsAppErrorVO error)
    {
        Validation(error);

        return await _dao.PersistAsync(error);
    }

    private void Validation(WhatsAppErrorVO error)
    {

    }
}
