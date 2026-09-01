using CommonBusiness.Extensions;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness;

public class WhatsAppCredentialsBE
{
    WhatsAppCredentialsDAO _dao = new WhatsAppCredentialsDAO();
    public async Task<WhatsAppCredentialsVO> GetNew()
    {
        WhatsAppCredentialsVO whatsAppCredentials = await _dao.GetNextCodeNumber<WhatsAppCredentialsVO>();

        return whatsAppCredentials;
    }

    public async Task<IList<WhatsAppCredentialsVO>> GetAllWhatsAppCredentials()
    {
        return await _dao.GetAllWhatsAppCredentials();
    }
}
