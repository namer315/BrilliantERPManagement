using AutoMapper;
using Microsoft.Extensions.Logging;
using WhatsAppData.DTO.Chat;

namespace WhatsAppData.DTO;

public class DTOHelper
{

    private static ILoggerFactory loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
    public static IMapper mapper = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<ChatMessageProfile>();
        cfg.AddProfile<ChatMessageMediaProfile>();
        cfg.AddProfile<ChatMessageButtonProfile>();
    } , loggerFactory).CreateMapper();

}
    
