using AutoMapper;
using Microsoft.Extensions.Logging;

namespace WhatsAppDTO.Management;

public class ManagerBase
{
    private static ILoggerFactory loggerFactory = new LoggerFactory();
    public static IMapper mapper = new MapperConfiguration(cfg =>
    {
        //cfg.AddProfile<AccountProfile>();
    } , loggerFactory).CreateMapper();

}

