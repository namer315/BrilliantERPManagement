using CommonData.Managers;
using CommonData.VO;
using System.Collections;
using WhatsAppData.DAO;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppData.Managers;
using WhatsAppData.Search.Chat;
using WhatsAppData.VO.WhatsApp;
using static WhatsAppData.DTO.Chat.ChatMessageDTO;

namespace WhatsAppBusiness.WhatsApp;

public class MessageBE
{
    MessageDAO _dao = new MessageDAO();

    internal async Task<MessageVO> getMessageBy(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
            throw new ArgumentException("messageId cannot be null or empty." , nameof(messageId));

        MessageVO message = await _dao.GetMessageBy(messageId);

        if (message is null)
            throw new KeyNotFoundException($"No message found for ID '{messageId}'.");

        return message;
    }

    internal async Task<MessageVO> GetNew(MessageVO.WhatsAppMessageTypes type , MessageDirections direction , ContactVO contact = null! , string messageId = null! , TenantVO tenant = null!)
    {
        MessageVO message = new MessageVO();

        message.MessageId = messageId;
        message.Type = type;

        message.MessageDirection = direction;

        if (direction == MessageDirections.Incoming)
            message.Receiver = contact;
        else
        {
            if (WhatsAppTenantManager.IskeyExist)
                message.Sender = WhatsAppTenantManager.CurrentContact;
            else
                message.Sender = contact;
        }

        if (TenantManager.IskeyExist)
            message.Tenant = TenantManager.CurrentTenant;
        else if (tenant is not null)
            message.Tenant = tenant;

        return message;
    }

    internal async Task<string> Persist(MessageVO message , bool merge = false)
    {
        Validation(message);

        return await _dao.MergeAsync(message);
    }

    private void Validation(MessageVO message)
    {
        if (message.Id != Guid.Empty)
        {
            message.UpdatedAt = DateTime.UtcNow;
        }
        if (message.MessageDirection == MessageDirections.Incoming && message.Sender is null)
            throw new ArgumentException("Incoming messages must have a Sender contact." , nameof(message));

        //if(message.MessageDirection == MessageDirections.Outgoing && message.Receiver is null)
        //    throw new ArgumentException("Outgoing messages must have a Receiver contact." , nameof(message));
        if (message.MessageDirection == MessageDirections.Outgoing && message.Sender is null)
            throw new ArgumentException("Outgoing messages must have a Sender contact." , nameof(message));
        if (message.MessageDirection == MessageDirections.Outgoing && message.Tenant is null)
            throw new ArgumentException("Outgoing messages must have a Tenant." , nameof(message));
    }

    public async Task<MessageVO> GetLastMessageBySender(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("number cannot be null or empty." , nameof(number));

        return await _dao.GetLastMessageBySender(number);
    }

    // "1723467890" -> DateTime
    public static DateTime ToDateTime(long webhookTimestamp)
    {
        //long seconds = long.Parse(webhookTimestamp);
        var dt = DateTimeOffset.FromUnixTimeSeconds(webhookTimestamp).UtcDateTime;
        //Console.WriteLine($"[ToDateTime] webhookTimestamp: {webhookTimestamp} -> DateTime: {dt:yyyy-MM-dd HH:mm:ss} UTC");
        return dt;
    }

    // "1723467890" -> TimeSpan since that time
    public static TimeSpan ToTimeSpanSince(long webhookTimestamp)
    {
        var date = ToDateTime(webhookTimestamp);
        var elapsed = DateTime.UtcNow - date;
        //Console.WriteLine($"[ToTimeSpanSince] Since {date:yyyy-MM-dd HH:mm:ss} = {elapsed.Days}d {elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s ago");
        return elapsed;
    }

    // "1723467890" -> how much left in 24h window
    public static TimeSpan TimeLeftInSession(long webhookTimestamp)
    {
        var elapsed = ToTimeSpanSince(webhookTimestamp);
        var total = TimeSpan.FromHours(24);
        var left = total - elapsed;
        if (left < TimeSpan.Zero) left = TimeSpan.Zero;

        //Console.WriteLine($"[TimeLeftInSession] 24h window - elapsed {elapsed} = left {left}");
        //Console.WriteLine($"[TimeLeftInSession] Can send free text: {left > TimeSpan.Zero}");

        return left;
    }
    public bool IsIn24hSession(long webhookTimestamp)
    {
        var left = TimeLeftInSession(webhookTimestamp);
        bool isInSession = left > TimeSpan.Zero;

        //Console.WriteLine($"[IsIn24hSession] timestamp {webhookTimestamp} -> IsInSession: {isInSession}");
        return isInSession;
    }
    //public async Task<bool> IsIn24hSession(string number)
    //{
    //    var lastMessage = await GetLastMessageBySender(number);

    //    // No inbound message ever recorded → not in an open session
    //    if (lastMessage?.Timestamp is not long ts)
    //        return false;

    //    // MessageBE.IsIn24hSession expects the raw Unix-seconds timestamp
    //    return IsIn24hSession(ts.ToString());
    //}


    public async Task<SessionCheckResponseDTO> Check24hSession(string phone)
    {
        MessageVO lastMessage = await GetLastMessageBySender(phone);

        if (lastMessage is null || lastMessage.Timestamp is null)
        {
            return new SessionCheckResponseDTO
            {
                Phone = phone ,
                IsIn24hSession = false
            };
        }

        bool isInSession = IsIn24hSession(lastMessage.Timestamp.Value);

        return new SessionCheckResponseDTO
        {
            Phone = phone ,
            IsIn24hSession = isInSession ,
            LastMessageAt = isInSession && lastMessage.Timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(lastMessage.Timestamp.Value) : null
        };
    }

    internal async Task<TenantVO> GetTenantbyContact(ContactVO sender)
    {
        if (sender is null)
            throw new ArgumentNullException(nameof(sender));
        if (sender.Id == Guid.Empty)
            throw new ArgumentException("Sender's Id cannot be empty." , nameof(sender));

        IList<TenantVO> tenantList = await _dao.GetTenantsByContact(sender);
        if (tenantList is { Count: > 0 })
            return tenantList.FirstOrDefault();
        else
            return null;

        //if(tenantList is { Count: > 1 })
        //    throw new InvalidOperationException($"Multiple tenants found for contact '{sender.Id}'.");

        //return tenantList.FirstOrDefault() ?? sender.Tenant;
    }

    internal async Task<IList<MessageVO>> GetMessageHistoryBy(Guid contactId , ChatHistorySH chatHistorySH)
    {
        IList rowData = await _dao.GetMessageHistoryBySelect(contactId , chatHistorySH);
        IList<MessageVO> messageList = new List<MessageVO>();

        foreach (object[] raw in rowData)
        {
            int index = 0;

            MessageVO message = new MessageVO();
            // --- message scalar fields ---
            message.Id = (Guid)raw[index++];
            message.MessageId = Convert.ToString(raw[index++]);
            message.Content = Convert.ToString(raw[index++]);
            message.Status = Convert.ToString(raw[index++]);
            message.Timestamp = raw[index++] as long?;
            message.Type = (MessageVO.WhatsAppMessageTypes)raw[index++];
            message.MessageDirection = (MessageDirections)raw[index++];
            message.CreatedAt = Convert.ToDateTime(raw[index++]);

            // --- media ---
            if(raw[index] is not null)
            {
                message.Media = new MessageMediaVO();
                message.Media.Id = (Guid)raw[index++];
                message.Media.FileName = Convert.ToString(raw[index++]);
                message.Media.Type = (MessageMediaVO.MediaTypes)raw[index++];
            }
            else
                index += 3;

            // --- sender ---
            if (raw[index] is not null)
            {
                message.Sender = new ContactVO();
                message.Sender.Id = (Guid)raw[index++];
                message.Sender.Name = Convert.ToString(raw[index++]);
                message.Sender.WaId = Convert.ToString(raw[index++]);
            }
            else
                index += 3;

            // --- receiver ---
            if (raw[index] is not null)
            {
                message.Receiver = new ContactVO();
                message.Receiver.Id = (Guid)raw[index++];
                message.Receiver.Name = Convert.ToString(raw[index++]);
                message.Receiver.WaId = Convert.ToString(raw[index++]);
            }
            else
                index += 3;

            // --- tenant ---
            if (raw[index] is not null)          // 18 tenent.Id
            {
                message.Tenant = new TenantVO();
                message.Tenant.Id = (Guid)raw[index++];
            }
            //else
            //    index += 1;

            messageList.Add(message);
        }

        return messageList;
    }
}
