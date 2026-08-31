using CommonBusiness;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DAO;
using WhatsAppData.VO;

namespace WhatsAppBusiness;

public class WhatsAppTenantBE
{
    public WhatsAppTenantDAO _dao = new WhatsAppTenantDAO();
    public TenantBE tenantBE = new TenantBE();
    public ContactBE contactBE = new ContactBE();
    public async Task CreateNewTenet(WhatsAppTenantVO whatsAppTenant)
    {
        Validation(whatsAppTenant);
        //WhatsAppTenantVO whatsAppTenant = new WhatsAppTenantVO();
        //whatsAppTenant.MapTo<WhatsAppTenantVO>();

        await _dao.PersistAsync(whatsAppTenant);
    }

    private void Validation(WhatsAppTenantVO whatsAppTenant)
    {
        //check if name is exist in the DataBase
        tenantBE.Validation(whatsAppTenant.Tenant);
        contactBE.Persist(whatsAppTenant.Contact);

        if (string.IsNullOrEmpty(whatsAppTenant.WABusinessAccountId))
            throw new ArgumentException("The WhatsApp Business Account ID is required and must not be empty. Please provide a valid Business Account ID before proceeding.", nameof(whatsAppTenant.WABusinessAccountId));
        if (string.IsNullOrEmpty(whatsAppTenant.WAAccessToken))
            throw new ArgumentException("The WhatsApp Access Token is required and must not be empty. Please provide a valid Access Token to authenticate with the WhatsApp Business API.", nameof(whatsAppTenant.WAAccessToken));
        if (string.IsNullOrEmpty(whatsAppTenant.WAPhoneNumberId))
            throw new ArgumentException("The WhatsApp Phone Number ID is required and must not be empty. Please provide the Phone Number ID associated with the registered WhatsApp Business account.", nameof(whatsAppTenant.WAPhoneNumberId));

    }

    public async Task<IList<WhatsAppTenantVO>> GetAllWhatsAppTenants()
    {
        var rawData = await _dao.GetAllAsync();
        IList<WhatsAppTenantVO> tenants = new List<WhatsAppTenantVO>();
        
        foreach (object[] item in rawData)
        {
            int index = 0;
            var id = (Guid)item[index++];
            var accountId = Convert.ToString(item[index++]);
            var phoneId = Convert.ToString(item[index++]);
            var tenantName = item[index++];
            var contactName = item[index++];
            var contactPhone = item[index++];

            tenants.Add(new WhatsAppTenantVO
            {
                Id = id,
                WABusinessAccountId = accountId,
                WAPhoneNumberId = phoneId,
                Tenant = tenantName != null ? new CommonData.VO.TenantVO { Name = Convert.ToString(tenantName) } : null,
                Contact = contactName != null ? new WhatsAppData.VO.WhatsApp.ContactVO { Name = Convert.ToString(contactName), PhoneNumber = Convert.ToString(contactPhone) } : null
            });
        }
        
        return tenants;
    }

    public async Task<WhatsAppTenantVO> GetWhatsAppTenantById(Guid id)
    {
        return await _dao.GetByIdAsync(id);
    }
}
