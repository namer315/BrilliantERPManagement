using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

internal class MessageStatusBE
{
    MessageStatusDAO _dao = new MessageStatusDAO();
    internal MessageStatusVO GetNew(MessageVO message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        MessageStatusVO messageStatus = new MessageStatusVO();
        messageStatus.Message = message;

        return messageStatus;
    }

    internal async Task<string> Persist(MessageStatusVO messageStatus)
    {
        Validation(messageStatus);

        return await _dao.PersistAsync(messageStatus);
    }

    private void Validation(MessageStatusVO messageStatus)
    {
        throw new NotImplementedException();
    }
}
