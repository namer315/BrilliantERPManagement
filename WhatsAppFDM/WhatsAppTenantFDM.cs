using WhatsAppBusiness;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppFDM;

public class WhatsAppTenantFDM
{
    WhatsAppTenantBE _be = new WhatsAppTenantBE();
    public async Task<WhatsAppTenantVO> GetBy(Guid id) => await new WhatsAppTenantDAO().GetByIdAsync(id);

    public async Task<WhatsAppTenantVO> GetByTenantId(Guid tenantId) => await new WhatsAppTenantDAO().GetByTenantIdAsync(tenantId);

    public async Task<WhatsAppTenantVO> GetNew() => await _be.GetNew();

    public async Task Persist(WhatsAppTenantVO whatsAppTenant) => await _be.Persist(whatsAppTenant);

    public async Task<IList<WhatsAppTenantVO>> GetAllActiveWhatsAppTenants() => await _be.GetAllActiveWhatsAppTenants();

    public async Task<IList<WhatsAppTenantVO>> GetAllInactiveWhatsAppTenants() => await _be.GetAllInactiveWhatsAppTenants();

    public async Task<ContactVO> GetContactBy(string waId) => await new ContactDAO().GetContactBy(waId.Trim());
}
