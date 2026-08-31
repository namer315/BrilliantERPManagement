using CommonBusiness;
using CommonData.VO;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

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

        if (string.IsNullOrEmpty(whatsAppTenant.WhatsAppCredentials.WABusinessAccountId))
            throw new ArgumentException("The WhatsApp Business Account ID is required and must not be empty. Please provide a valid Business Account ID before proceeding.", nameof(whatsAppTenant.WhatsAppCredentials.WABusinessAccountId));
        if (string.IsNullOrEmpty(whatsAppTenant.WhatsAppCredentials.WAAccessToken))
            throw new ArgumentException("The WhatsApp Access Token is required and must not be empty. Please provide a valid Access Token to authenticate with the WhatsApp Business API.", nameof(whatsAppTenant.WhatsAppCredentials.WAAccessToken));
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
            WhatsAppTenantVO whatsAppTenant = new WhatsAppTenantVO();
            whatsAppTenant.Id = (Guid)item[index++];
            whatsAppTenant.WhatsAppCredentials = new WhatsAppCredentialsVO();
            whatsAppTenant.WhatsAppCredentials.WABusinessAccountId = Convert.ToString(item[index++]);
            whatsAppTenant.WAPhoneNumberId = Convert.ToString(item[index++]);

            whatsAppTenant.Tenant = new TenantVO();
            whatsAppTenant.Tenant.Name = Convert.ToString(item[index++]);

            whatsAppTenant.Contact = new ContactVO();
            //whatsAppTenant.Contact.Name = Convert.ToString(item[index++]);
            whatsAppTenant.Contact.WaId = Convert.ToString(item[index++]);

            tenants.Add(whatsAppTenant);
        }
        
        return tenants;
    }

    public async Task<WhatsAppTenantVO> GetWhatsAppTenantById(Guid id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task DisableWhatsAppTenant(Guid id)
    {
        var tenant = await _dao.GetByIdAsync(id);
        if (tenant?.Tenant == null)
            throw new ArgumentException("WhatsApp Tenant not found.", nameof(id));

        tenant.Tenant.Active = false;
        await _dao.PersistAsync(tenant);
    }

    public async Task EnableWhatsAppTenant(Guid id)
    {
        var tenant = await _dao.GetByIdAsync(id);
        if (tenant?.Tenant == null)
            throw new ArgumentException("WhatsApp Tenant not found.", nameof(id));

        tenant.Tenant.Active = true;
        await _dao.PersistAsync(tenant);
    }

    public async Task<IList<WhatsAppTenantVO>> GetAllActiveWhatsAppTenants()
    {
        return await _dao.GetAllActiveAsync();
    }

    public async Task<IList<WhatsAppTenantVO>> GetAllInactiveWhatsAppTenants()
    {
        return await _dao.GetAllInactiveAsync();
    }

    public async Task SaveOrUpdateTenet(WhatsAppTenantVO whatsAppTenant)
    {
        Validation(whatsAppTenant);
        await _dao.PersistAsync(whatsAppTenant);
    }
}
