using FastEndpoints;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppFDM.WhatsApp;
using static BrilliantWhatsAppAPI.Endpoints.SessionCheckEP;

namespace BrilliantWhatsAppAPI.Endpoints;

public class SessionCheckEP : Endpoint<SessionCheckDTO , SessionCheckResponseDTO>
//public class SessionCheckEP : EndpointWithoutRequest<SessionCheckResponseDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();
    public override void Configure()
    {
        Get("Sessions/24h");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Check 24h session status by phone number";
            //s.Params["phone"] = "The phone number to query"; // Adds 'phone' as a query parameter in Swagger
            //s.RequestParam(p => p.Phone , "The phone number to query"); // Optional parameter description
        });
    }

    public async override Task<SessionCheckResponseDTO> ExecuteAsync(SessionCheckDTO req , CancellationToken ct)
    //public async override Task<SessionCheckResponseDTO> ExecuteAsync(CancellationToken ct)
    {
        var phone = Query<string>("phone");
        return await _fdm.Check24hSession(phone);
    }

    public class SessionCheckDTO
    {
        [QueryParam]
        public string Phone { get; set; } = string.Empty;
    }
}
