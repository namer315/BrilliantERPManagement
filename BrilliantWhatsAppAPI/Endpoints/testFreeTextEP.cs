using BrilliantWhatsAppAPI.DTO;
using BrilliantWhatsAppAPI.Management;
using FastEndpoints;
using Org.BouncyCastle.Asn1.X509;

namespace BrilliantWhatsAppAPI.Endpoints;

/*public class testTextEP : Endpoint<tTextMessageDTO , dynamic>
{
    WhatsAppHelper _whatsAppHelper = new WhatsAppHelper();

    public override void Configure()
    {
        Post("test/Text");
        AllowAnonymous();
    }
    public async override Task<dynamic> ExecuteAsync(tTextMessageDTO req , CancellationToken ct)
    {
        return await _whatsAppHelper.SendMessageAsync(req);
        /* //string phoneNumber = "27613009425";
        string phoneNumber = "0742420893";
        req.PhoneNumber = phoneNumber;
        await _whatsAppHelper.SendMessageAsync(req);
        //return await _whatsAppHelper.SendImageMessageAsync(req);
        await _whatsAppHelper.SendMessageAsync(new tTextMessageDTO
        {
            PhoneNumber = phoneNumber ,
            Message = "Choose shipping" ,
            InteractiveType = "list" ,
            HeaderText = "Shipping Options" ,
            Footer = "Lucky Shrub™" ,
            ListButtonText = "View Options" ,
            ListSections = new List<tListSectionDTO>
            {
                new() 
                { 
                    Title = "Fast", Rows = new List<tListRowDTO>
                    {
                        new() { Id = "p1", Title = "Priority 1", Description = "1-2 days" } ,
                        new() { Id = "p2", Title = "Priority 2", Description = "3-4 days" } ,
                        new() { Id = "p3", Title = "Priority 3", Description = "5-6 days" } ,
                    }
                }
            }
        });
        await _whatsAppHelper.SendMessageAsync(new tTextMessageDTO
        {
            PhoneNumber = phoneNumber ,
            Message = "Tap for dates" ,
            InteractiveType = "cta_url" ,
            CtaDisplayText = "See Dates" ,
            CtaUrl = "https://example.com/dates"
        });

        return await _whatsAppHelper.SendMessageAsync(new tTextMessageDTO
        {
            PhoneNumber = phoneNumber ,
            Message = "Share your location" ,
            InteractiveType = "location"
        });* /
    }
}
*/