using CommonData.Managers;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

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

    internal async Task<MessageVO> GetNew(MessageVO.WhatsAppMessageTypes type , string messageId = null)
    {
        MessageVO message = new MessageVO();

        message.MessageId = messageId;
        message.Type = type;
        if(TenantManager.IskeyExist)
            message.Tenant = TenantManager.CurrentTenant;

        return message;
    }

    internal async Task<string> Persist(MessageVO message , bool merge = false)
    {
        Validation(message);

        if (merge)
            return await _dao.MergeAsync(message);
        else
            return await _dao.PersistAsync(message);
    }

    private void Validation(MessageVO message)
    {

    }

    public async Task GetLastMessageBySender(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("number cannot be null or empty." , nameof(number));

        MessageVO message = await _dao.GetLastMessageBySender(number);

    }

    // "1723467890" -> DateTime
    public static DateTime ToDateTime(string webhookTimestamp)
    {
        long seconds = long.Parse(webhookTimestamp);
        var dt = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        //Console.WriteLine($"[ToDateTime] webhookTimestamp: {webhookTimestamp} -> DateTime: {dt:yyyy-MM-dd HH:mm:ss} UTC");
        return dt;
    }

    // "1723467890" -> TimeSpan since that time
    public static TimeSpan ToTimeSpanSince(string webhookTimestamp)
    {
        var date = ToDateTime(webhookTimestamp);
        var elapsed = DateTime.UtcNow - date;
        //Console.WriteLine($"[ToTimeSpanSince] Since {date:yyyy-MM-dd HH:mm:ss} = {elapsed.Days}d {elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s ago");
        return elapsed;
    }

    // "1723467890" -> how much left in 24h window
    public static TimeSpan TimeLeftInSession(string webhookTimestamp)
    {
        var elapsed = ToTimeSpanSince(webhookTimestamp);
        var total = TimeSpan.FromHours(24);
        var left = total - elapsed;
        if (left < TimeSpan.Zero) left = TimeSpan.Zero;

        //Console.WriteLine($"[TimeLeftInSession] 24h window - elapsed {elapsed} = left {left}");
        //Console.WriteLine($"[TimeLeftInSession] Can send free text: {left > TimeSpan.Zero}");

        return left;
    }
    public static bool IsIn24hSession(string webhookTimestamp)
    {
        var left = TimeLeftInSession(webhookTimestamp);
        bool isInSession = left > TimeSpan.Zero;

        //Console.WriteLine($"[IsIn24hSession] timestamp {webhookTimestamp} -> IsInSession: {isInSession}");
        return isInSession;
    }
}
