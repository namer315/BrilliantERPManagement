using CommonBusiness;
using CommonData.VO;
using System.Threading.Tasks;

namespace CommonFDM;

public class TenantFDM
{
    TenantBE _be = new TenantBE();

    public async Task<TenantVO> ResolveTenantByToken(string token) =>  await _be.ResolveTenantByToken(token);
        
}
