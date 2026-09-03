using CommonBusiness;
using CommonData.VO;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;
using CommonBusiness.Extensions;

namespace WhatsAppBusiness;

public class WhatsAppTenantBE
{
    private WhatsAppTenantDAO _dao = new WhatsAppTenantDAO();
    private TenantBE _tenantBE = new TenantBE();
    private ContactBE _contactBE = new ContactBE();
    private WhatsAppCredentialsBE _whatsAppCredentialsBE = new WhatsAppCredentialsBE();

    public async Task Persist(WhatsAppTenantVO whatsAppTenant)
    {
        await Validation(whatsAppTenant);
        //WhatsAppTenantVO whatsAppTenant = new WhatsAppTenantVO();
        //whatsAppTenant.MapTo<WhatsAppTenantVO>();

        await _dao.MergeAsync(whatsAppTenant);
    }

    private async Task Validation(WhatsAppTenantVO whatsAppTenant)
    {
        //check if name is exist in the DataBase
        _tenantBE.Validation(whatsAppTenant.Tenant);

        if (await new ContactDAO().GetContactBy(whatsAppTenant.Contact.WaId) is ContactVO contact)
            whatsAppTenant.Contact = contact;

        //_contactBE.Persist(whatsAppTenant.Contact);

        if (string.IsNullOrEmpty(whatsAppTenant.WhatsAppCredentials.WABusinessAccountId))
            throw new ArgumentException("The WhatsApp Business Account ID is required and must not be empty. Please provide a valid Business Account ID before proceeding.", nameof(whatsAppTenant.WhatsAppCredentials.WABusinessAccountId));
        if (string.IsNullOrEmpty(whatsAppTenant.WhatsAppCredentials.WAAccessToken))
            throw new ArgumentException("The WhatsApp Access Token is required and must not be empty. Please provide a valid Access Token to authenticate with the WhatsApp Business API.", nameof(whatsAppTenant.WhatsAppCredentials.WAAccessToken));
        if (string.IsNullOrEmpty(whatsAppTenant.Contact.PhoneNumberId))
            throw new ArgumentException("The WhatsApp Phone Number ID is required and must not be empty. Please provide the Phone Number ID associated with the registered WhatsApp Business account.", nameof(whatsAppTenant.Contact.PhoneNumberId));

    }

    public async Task<IList<WhatsAppTenantVO>> GetAllWhatsAppTenants()
    {
        var rawData = await _dao.GetAllAsync();
        IList<WhatsAppTenantVO> tenants = new List<WhatsAppTenantVO>();
        
        int index = 0;
        foreach (object[] item in rawData)
        {
            index = 0;
            WhatsAppTenantVO whatsAppTenant = new WhatsAppTenantVO();
            whatsAppTenant.Id = (Guid)item[index++];

            whatsAppTenant.WhatsAppCredentials = new WhatsAppCredentialsVO();
            whatsAppTenant.WhatsAppCredentials.WABusinessAccountId = Convert.ToString(item[index++]);

            whatsAppTenant.Tenant = new TenantVO();
            whatsAppTenant.Tenant.Name = Convert.ToString(item[index++]);
            whatsAppTenant.Tenant.Active = Convert.ToBoolean(item[index++]);

            whatsAppTenant.Contact = new ContactVO();
            //whatsAppTenant.Contact.Name = Convert.ToString(item[index++]);
            whatsAppTenant.Contact.WaId = Convert.ToString(item[index++]);
            whatsAppTenant.Contact.PhoneNumberId = Convert.ToString(item[index++]);

            tenants.Add(whatsAppTenant);
        }
        
        return tenants;
    }

    public async Task<WhatsAppTenantVO> GetWhatsAppTenantById(Guid id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task SetWhatsAppTenantActive(Guid id, bool active)
    {
        var tenant = await _dao.GetByIdAsync(id);
        if (tenant?.Tenant == null)
            throw new ArgumentException("WhatsApp Tenant not found.", nameof(id));

        tenant.Tenant.Active = active;
        await Persist(tenant);
    }

    public async Task<IList<WhatsAppTenantVO>> GetAllActiveWhatsAppTenants()
    {
        return await _dao.GetByActiveAsync(true);
    }

    public async Task<IList<WhatsAppTenantVO>> GetAllInactiveWhatsAppTenants()
    {
        return await _dao.GetByActiveAsync(false);
    }

    public async Task SaveOrUpdateTenet(WhatsAppTenantVO whatsAppTenant)
    {
        Validation(whatsAppTenant);
        await _dao.PersistAsync(whatsAppTenant);
    }

    public async Task<WhatsAppTenantVO> GetNew()
    {
        WhatsAppTenantVO whatsAppTenant = await _dao.GetNextCodeNumber<WhatsAppTenantVO>();
        
        // Ensure nested objects are initialized
        whatsAppTenant.Tenant = await _tenantBE.GetNew();
        whatsAppTenant.Contact = await _contactBE.GetContactBy("");
        whatsAppTenant.WhatsAppCredentials = await _whatsAppCredentialsBE.GetNew();

        return whatsAppTenant;
    }

    internal async Task<TenantVO> GetTenantBy(ContactVO contact)
    {
        var count = await _dao.GetCountBy(contact);
        TenantVO tenant = null;
        if (count == 1)
            tenant = await _dao.GetTenantBy(contact);

        return tenant;
    }
}
