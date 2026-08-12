using CommonData.Managers;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class MessageBE
{
    MessageDAO _dao = new MessageDAO();

    internal async Task<MessageVO> GetNew(MessageVO.WhatsAppMessageTypes type)
    {
        MessageVO message = new MessageVO();

        message.Type = type;
        message.Tenant = TenantManager.CurrentTenant;

        return message;
    }

    internal async Task<string> Persist(MessageVO message, bool merge = false)
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
