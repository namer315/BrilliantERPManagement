using AutoMapper;
using Microsoft.Extensions.Logging;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.Common;

namespace WhatsAppData.DTO;

public class DTOHelper
{

    private static ILoggerFactory loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
    public static IMapper mapper = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<ChatMessageProfile>();
        cfg.AddProfile<ChatMessageMediaProfile>();
        cfg.AddProfile<ChatMessageButtonProfile>();
        cfg.AddProfile<ContactProfile>();
    } , loggerFactory).CreateMapper();

}
    
