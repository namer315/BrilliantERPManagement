using CommonData.Exceptions;
using WhatsAppData.DTO.Webhooks;

namespace WhatsAppData.Exceptions;

public class WhatsAppApiException : AppException
{
    public WhatsAppApiException(ErrorDTO? error)
        : base(
            message: error?.Message ?? "WhatsApp Cloud API returned an error." ,
            type: AppErrorType.ExternalService ,
            code: error is null ? "WHATSAPP_API_ERROR" : $"WHATSAPP_{error.Code}" ,
            details: error is null
                ? null
                : new Dictionary<string , object?>
                {
                    ["type"] = error.Type ,
                    ["title"] = error.Title ,
                    ["fbtraceId"] = error.FbTraceId ,
                    ["href"] = error.Href
                })
    {
    }
}
