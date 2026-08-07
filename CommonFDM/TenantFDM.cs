using CommonBusiness;
using CommonData.VO;

namespace CommonFDM;

public class TenantFDM
{
    TenantBE _be = new TenantBE();

    public TenantVO ResolveTenantByToken(string token)
    {
        return _be.ResolveTenantByToken(token);
    }
}
