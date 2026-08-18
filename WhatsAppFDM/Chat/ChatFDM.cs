using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Common;

namespace WhatsAppFDM.Chat;

public class ChatFDM
{
    public async Task<IList<ContactDTO>> GetChatsContactList()
    {
        return await new ContactBE().GetChatsContactList();
    }
}
