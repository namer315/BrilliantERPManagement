using CommonData.Extensions;
using CommonData.Managers;
using CommonData.VO;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.Stream;
using WhatsAppData.DTO.Webhooks;
using WhatsAppData.VO.WhatsApp;
using static WhatsAppData.VO.WhatsApp.MessageStatusVO;

namespace WhatsAppBusiness.Webhooks;

public class WebhookBE
{
    private MessageBE _messageBE = new MessageBE();
    private MessageStatusBE _messageStatus = new MessageStatusBE();
    private ContactBE _contactBE = new ContactBE();
    private WhatsAppErrorBE _whatsAppErrorBE = new WhatsAppErrorBE();
    private WhatsAppPricingBE _whatsAppPricingBE = new WhatsAppPricingBE();

    public async Task<bool> HandleWebhook(WebhookDTO webhook)
    {
        try
        {
            //bool shouldSaveRequestToFile = false; // Set this flag based on your logic
            // Implementation for handling webhook
            string s;
            if (!webhook.Object.Equals("whatsapp_business_account") || webhook.Entry is not { Count: > 0 })
                return true;

            foreach (EntryDTO entry in webhook.Entry)
            {
                if (entry.Changes is not { Count: > 0 })
                    return true;

                foreach (ChangeDTO change in entry.Changes)
                {
                    if (change.Value is null)
                        return true;

                    // Handle the change based on its type
                    switch (change.Field)
                    {
                        case "messages":
                        {
                            // Handle incoming messages
                            if (change.Value.Statuses is { Count: > 0 })
                            {
                                foreach (StatusDTO status in change.Value.Statuses)
                                {
                                    MessageVO message = await _messageBE.getMessageBy(status.Id);
                                    MessageStatusVO messageStatus = _messageStatus.GetNew(message);
                                    messageStatus.Status = status.Status.ToEnum<WhatsAppMessageStatus>();
                                    messageStatus.Timestamp = Convert.ToInt64(status.Timestamp);

                                    s = await _messageStatus.Persist(messageStatus);
                                    ChatMessageStatusDTO chatMessageStatus = new ChatMessageStatusDTO();
                                    chatMessageStatus.MessageId = messageStatus.Message.MessageId;
                                    chatMessageStatus.Status = messageStatus.Status;
                                    chatMessageStatus.Timestamp = messageStatus.Timestamp;

                                    if (status.Errors is { Count: > 0 })
                                    {
                                        foreach (ErrorDTO e in status.Errors)
                                        {
                                            WhatsAppErrorVO error = _whatsAppErrorBE.GetNew(messageStatus);
                                            error.ErrorCode = e.Code;
                                            error.Title = e.Title;
                                            error.Message = e.Message;
                                            error.Details = e.ErrorData?.Details ?? string.Empty;
                                            error.Href = e.Href;

                                            s = await _whatsAppErrorBE.Persist(error);

                                            chatMessageStatus.Error = new WhatsAppData.DTO.Common.ErrorDTO();
                                            chatMessageStatus.Error.ErrorCode = error.ErrorCode;
                                            chatMessageStatus.Error.Message = error.Message;
                                            chatMessageStatus.Error.Details = error.Details;


                                        }
                                    }
                                    if (status.Pricing is not null)
                                    {
                                        WhatsAppPricingVO pricing = _whatsAppPricingBE.GetNew(messageStatus);
                                        pricing.Billable = status.Pricing.Billable;
                                        pricing.PricingModel = status.Pricing.PricingModel;
                                        pricing.Type = status.Pricing.Type;
                                        pricing.Category = status.Pricing.Category;

                                        s = await _whatsAppPricingBE.Persist(pricing);

                                        // Pricing is informational (billable/model/category/type).
                                        // It is not currently mapped to a persistent column; log/capture for audit.
                                        Console.WriteLine($"[status] pricing id={status.Id} billable={status.Pricing.Billable} model={status.Pricing.PricingModel} type={status.Pricing.Type} category={status.Pricing.Category}");
                                    }

                                    if(message.Timestamp is null && (status.Status.Equals("sent") || status.Status.Equals("failed")))
                                    {
                                        message.Timestamp = messageStatus.Timestamp;
                                        s = await _messageBE.Persist(message , true);
                                    }

                                    if (message.Tenant is not null)
                                    {
                                        StreamDTO stream = new StreamDTO();
                                        stream.Token = message.Tenant.Token;
                                        stream.TenantName = message.Tenant.Name;
                                        stream.MessageStatus = chatMessageStatus;

                                        stream.MessageStatus.Contact = new WhatsAppData.DTO.Common.ContactDTO();
                                        stream.MessageStatus.Contact.WaId = message.Receiver.WaId;
                                        stream.MessageStatus.Contact.Id = message.Receiver.Id;
                                        //stream.Message = new StreamMessageDTO();
                                        //stream.Message.From = message.Sender.WaId;
                                        //stream.Message.Content = message.Content;
                                        //if (message.Timestamp is long timestamp)
                                        //    stream.Message.DateTimeUTC = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                                        // Assume req.Token carries the tenant token
                                        var evt = new WebhookEvent(message.Tenant , stream);
                                        await WebhookEventChannel.PublishAsync(evt);
                                    }
                                }
                            }

                            if (change.Value.Messages is { Count: > 0 })
                            {
                                foreach (MessageDTO msg in change.Value.Messages)
                                {

                                    MessageVO message = await _messageBE.GetNew(MessageVO.WhatsAppMessageTypes.Text, msg.Id);

                                    message.Sender = await _contactBE.GetContactBy(msg.From);
                                    if (string.IsNullOrEmpty(message.Sender.Name))
                                    {
                                        message.Sender.Name = change.Value.Contacts?.FirstOrDefault(x => x.WaId.Equals(message.Sender.WaId))?.Profile?.Name;
                                    }

                                    if(msg.Text is not null)
                                    {
                                        message.Content = msg.Text.Body;
                                    }
                                    message.Timestamp = Convert.ToInt64(msg.Timestamp);

                                    s = await _messageBE.Persist(message , true);

                                    TenantVO tenant = await _messageBE.GetTenantbyContact(message.Sender);
                                    if(tenant is not null)
                                    {
                                        StreamDTO stream = new StreamDTO();
                                        stream.Token = tenant.Token;
                                        stream.TenantName = tenant.Name;

                                        stream.Message = new ChatMessageDTO();
                                        stream.Message.Body = message.Content;
                                        stream.Message.Timestamp = message.Timestamp;
                                        stream.Message.MessageDirection = ChatMessageDTO.MessageDirections.Incoming;

                                        stream.Message.Contact = new WhatsAppData.DTO.Common.ContactDTO();
                                        stream.Message.Contact.WaId = message.Sender.WaId;
                                        stream.Message.Contact.Id = message.Sender.Id;
                                        //stream.Message = new StreamMessageDTO();
                                        //stream.Message.From = message.Sender.WaId;
                                        //stream.Message.Content = message.Content;
                                        //if (message.Timestamp is long timestamp)
                                        //    stream.Message.DateTimeUTC = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                                        // Assume req.Token carries the tenant token
                                        var evt = new WebhookEvent(tenant , stream);
                                        await WebhookEventChannel.PublishAsync(evt);
                                    }
                                }
                            }
                        }
                            break;
                        case "statuses":
                            // Handle message status updates
                            break;
                        case "message_template_status_update":
                            // Handle message template status update
                            break;
                        default:
                            // Handle other types of changes if needed
                            break;
                    }
                }
            }

            return false;
        }
        catch(Exception ex)
        {
            throw ex;
            return true;
        }
    }

    public async IAsyncEnumerable<dynamic> Stream([EnumeratorCancellation] CancellationToken ct)
    {
        // Capture the authenticated tenant ONCE before any yield. The ambient
        // TenantContext is cleared by TenantScopeCleaner once the request scope
        // ends, so it must not be re-read inside the streaming loop.
        var currentTenant = TenantManager.CurrentTenant;

        // Each SSE connection subscribes to its own channel so events fan out to
        // every connected client (a single shared Channel is single-consumer).
        var subscriberId = WebhookEventChannel.Subscribe(out var channel);
        var reader = channel.Reader;

        try
        {
            // Notify the client immediately that it has started listening.
            yield return new StreamDTO
            {
                Token = currentTenant?.Token,
                TenantName = currentTenant?.Name,
                //Message = new StreamMessageDTO
                //{
                //    From = "system",
                //    Content = "Your session has been connected successfully.",
                //    DateTimeUTC = DateTimeOffset.UtcNow
                //}
            };

            while (!ct.IsCancellationRequested)
            {
                WebhookEvent? evt = null;
                bool hadError = false;
                bool heartbeat = false;

                try
                {
                    // Try to read with timeout so we can send heartbeats
                    //evt = await reader.ReadAsync(ct);
                    // Try to read with timeout so we can send heartbeats
                    var readTask = reader.ReadAsync(ct).AsTask();
                    var completed = await Task.WhenAny(readTask , Task.Delay(1000 * 10 , ct));

                    if (completed == readTask)
                    {
                        evt = await readTask; // got event
                    }
                    else
                    {
                        heartbeat = true; // timeout → heartbeat
                    }
                }
                catch (OperationCanceledException ex)
                {
                    WriteExceptionToLog(ex);
                    //yield break; // graceful shutdown
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                    hadError = true;
                }

                // Yield outside of try/catch
                if (evt != null && currentTenant != null && evt.Tenant.Id == currentTenant.Id)
                {
                    // Only deliver events belonging to this connection's tenant.
                    yield return evt.Data;
                }
                else if (hadError || heartbeat)
                {

                    //yield return new StreamDTO(); // keep connection alive
                    yield return ": ping"; // keep connection alive
                }
            }
        }
        finally
        {
            WebhookEventChannel.Unsubscribe(subscriberId);
        }
    }



    public static void WriteExceptionToLog(Exception ex)
    {
        if (ex == null) return;

        string logFolder = Path.Combine(AppContext.BaseDirectory , "log");
        Directory.CreateDirectory(logFolder);

        string logFile = Path.Combine(logFolder , $"error-{DateTime.Now:yyyyMMdd}.log");
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}";

        File.AppendAllText(logFile , entry);
    }
}

public record WebhookEvent(TenantVO Tenant , StreamDTO Data);

public static class WebhookEventChannel
{
    // Each connected SSE client gets its own unbounded channel (single-consumer
    // per client). Public events are fanned out to every subscriber channel.
    private static readonly ConcurrentDictionary<Guid, Channel<WebhookEvent>> _subscribers = new();

    /// <summary>
    /// Registers a new subscriber channel for a connected client.
    /// </summary>
    /// <returns>An id that must be passed to <see cref="Unsubscribe"/> on disconnect.</returns>
    public static Guid Subscribe(out Channel<WebhookEvent> channel)
    {
        channel = Channel.CreateUnbounded<WebhookEvent>(
            new UnboundedChannelOptions { SingleReader = true , SingleWriter = false });
        var id = Guid.NewGuid();
        _subscribers.TryAdd(id , channel);
        return id;
    }

    /// <summary>
    /// Removes and completes a subscriber channel when its connection is closed.
    /// </summary>
    public static void Unsubscribe(Guid id)
    {
        if (_subscribers.TryRemove(id , out var channel))
            channel.Writer.TryComplete();
    }

    /// <summary>
    /// Fans the event out to every connected subscriber channel. Consumers are
    /// expected to filter events by tenant before delivering to their client.
    /// </summary>
    public static async Task PublishAsync(WebhookEvent evt)
    {
        foreach (var (id , channel) in _subscribers)
        {
            await channel.Writer.WriteAsync(evt);
        }
    }
}
