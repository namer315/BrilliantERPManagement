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
        if(messageList is { Count:> 0 })
            messageList = messageList.OrderByDescending(x => x.CreatedAt).ToList();

        chatHistory.ChatMessagList = messageList.Select(x => new ChatMessageDTO()
        {
            Id = x.Id ,
            MessageId = x.MessageId ,
            Timestamp = x.Timestamp,
            MessageDirection = x.Sender?.WaId == chatHistorySH.WaId ? ChatMessageDTO.MessageDirections.Incoming : ChatMessageDTO.MessageDirections.Outgoing,
            Body = x.Content,
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

        IList<ContactDTO> contactDTOList = contactList
            .Select(x => new ContactDTO()
            {
                Id = x.Id ,
                WaId = x.WaId
            })
            .ToList();
        if (contactDTOList is not { Count: > 0 })
            return new List<ChatDTO>();

        return contactDTOList
            .Select(x => new ChatDTO()
            {
                Contact = x ,

            })
            .ToList();
        IList rawData = await new MessageDAO().GetLatestMessagesForContacts(contactDTOList.Select(c => c.Id).ToList());
        IList<ChatDTO> chatDTOList = new List<ChatDTO>();

        int index = 0;
        foreach (object[] row in rawData)
        {
            Guid senderId = Guid.Empty;
            Guid receiverId = Guid.Empty;
            if (row[index++] is Guid sender)
                senderId = sender;
            if (row[index++] is Guid receiver)
                receiverId = receiver;

            ChatDTO chatDTO = new ChatDTO();
            if (senderId != Guid.Empty)
                chatDTO.Contact = contactDTOList.FirstOrDefault(x => x.Id == senderId);
            if (chatDTO.Contact is null && receiverId != Guid.Empty)
                chatDTO.Contact = contactDTOList.FirstOrDefault(x => x.Id == receiverId);

            chatDTO.LastMessage = Convert.ToString(row[index++]);
            chatDTO.Timestamp = Convert.ToInt64(row[index++]);
            chatDTO.MessageId = Convert.ToString(row[index++]);

            chatDTOList.Add(chatDTO);
        }

        MessageVO message = new MessageVO();

        return chatDTOList;
    }
}
