using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Chat;

namespace WhatsAppFDM.Messages;

public class MessageFDM
{
    MessageBE _be = new MessageBE();
    //public async Task<ChatMessageDTO> GetMessageById(string messageId) 
    //    => await _be.GetMessageById(messageId);
    public async Task<ChatMessageDTO> GetMessageById(Guid id) => await _be.GetMessageById(id);
    
}
