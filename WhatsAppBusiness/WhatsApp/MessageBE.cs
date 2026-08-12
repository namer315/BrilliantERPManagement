using CommonData.Managers;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class MessageBE
{
    MessageDAO _dao = new MessageDAO();

    internal async Task<MessageVO> getMessageBy(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
            throw new ArgumentException("messageId cannot be null or empty." , nameof(messageId));

        MessageVO message = await _dao.GetMessageBy(messageId);

        if (message is null)
            throw new KeyNotFoundException($"No message found for ID '{messageId}'.");

        return message;
    }

    internal async Task<MessageVO> GetNew(MessageVO.WhatsAppMessageTypes type)
    {
        MessageVO message = new MessageVO();

        message.Type = type;
        message.Tenant = TenantManager.CurrentTenant;

        return message;
    }

    internal async Task<string> Persist(MessageVO message , bool merge = false)
    {
        Validation(message);

        if (merge)
            return await _dao.MergeAsync(message);
        else
            return await _dao.PersistAsync(message);
    }

    private void Validation(MessageVO message)
    {

    }
}
