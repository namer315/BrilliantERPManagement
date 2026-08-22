using CommonData.Managers;
using System.Collections;
using WhatsAppData.DAO;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.Common;
using WhatsAppData.Search.Chat;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class ChatBE
{
    public async Task<ChatHistoryDTO> GetChatHistoryBy(ChatHistorySH chatHistorySH)
    {
        //if (string.IsNullOrEmpty(waId))
        //    throw new ArgumentException("WhatsApp ID cannot be null or empty." , nameof(waId));

        if (string.IsNullOrEmpty(chatHistorySH.WaId))
            throw new ArgumentNullException(nameof(chatHistorySH.WaId) , "WhatsApp ID is required.");

        ContactVO contact = await new ContactBE().GetContactBy(chatHistorySH.WaId);

        ChatHistoryDTO chatHistory = new ChatHistoryDTO();
        chatHistory.Chat = new ChatDTO()
        {
            Contact = new ContactDTO()
            {
                Id = contact.Id ,
                WaId = contact.WaId
            }
        };

        // Fetch message history from your data source
        IList<MessageVO> messageList = await new MessageDAO().GetMessageHistoryBy(contact.Id, chatHistorySH);
        if(string.IsNullOrEmpty(chatHistorySH.MessageId) && messageList is { Count:> 0 })
            messageList = messageList.OrderBy(x => x.CreatedAt).ToList();

        chatHistory.ChatMessagList = messageList.Select(x => new ChatMessageDTO()
        {
            Id = x.Id ,
            MessageId = x.MessageId ,
            Type = x.Type,
            Timestamp = x.Timestamp,
            MessageDirection = x.Sender?.WaId == chatHistorySH.WaId ? ChatMessageDTO.MessageDirections.Incoming : ChatMessageDTO.MessageDirections.Outgoing,
            Body = x.Content,
            Button = x.Button is not null ? new ChatMessageButtonDTO()
            {
                Id = x.Button.Id ,
                Text = x.Button.Text
            } : null ,
        })
            .ToList();
        foreach (var batch in chatHistory.ChatMessagList.Select(x => x.Id).ToList().Chunk(1900))
        {
            var rawDataStatus = await new MessageStatusDAO().GetMessageStatusBy(batch);
            foreach (ChatMessageDTO chatMessage in chatHistory.ChatMessagList)
            {
                (Guid Id , MessageStatusVO.WhatsAppMessageStatus Status) row = rawDataStatus.FirstOrDefault(s => s.Id == chatMessage.Id);
                if (row.Id != Guid.Empty)
                {
                    chatMessage.Status = row.Status;
                }
            }
        }
        return chatHistory;
    }

    public async Task<IList<ChatDTO>> GetChatsContactList()
    {
        if (!TenantManager.IskeyExist)
            throw new InvalidOperationException("Tenant key does not exist.");

        if (TenantManager.CurrentTenant is null)
            throw new InvalidOperationException("Current tenant is not set.");

        IList<ContactVO> contactList = await new ContactDAO().GetChatListContacts();

        if (contactList is not { Count: > 0 })
            return new List<ChatDTO>();

        IList<ChatDTO> chatList = contactList
            .Select(x => new ChatDTO(){ 
                Contact =  new ContactDTO()
                {
                    Id = x.Id ,
                    WaId = x.WaId
                }
            })
            .ToList();

        // Fetch lats message history from your data source
        IList<MessageVO> messageList = await new MessageDAO().GetLatestMessagesForContacts(chatList.Select(c => c.Contact.Id).ToList());
        chatList = chatList.Select(chat =>
        {
            // Find the latest message for this contact
            var lastMessage = messageList
                .FirstOrDefault(m => m.Sender?.Id == chat.Contact.Id || m.Receiver?.Id == chat.Contact.Id);
            if (lastMessage is null)
                return chat;

            return new ChatDTO
            {
                Contact = chat.Contact ,
                LastMessage = lastMessage?.Content ,
                Timestamp = lastMessage?.Timestamp ,
                MessageId = lastMessage?.MessageId
            };
        }).ToList();

        return chatList;
    }
}
