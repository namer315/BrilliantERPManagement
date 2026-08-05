using BrilliantWhatsAppAPI.DTO;
using System.Text.Json;

namespace BrilliantWhatsAppAPI.Management;

public class WhatsAppHelper
{
    /// <summary>
    /// {WHATSAPP_BUSINESS_ACCOUNT_ID} or {WABA_ID}
    /// </summary>
    private string _WhatsAppBusinessAccountId = "1550103273123935";
    private readonly string _phoneNumberId = "1124475747418242";
    private readonly string _accessToken = "EAARC64ekrboBRvMRrQ2gF3MZC1hopZCdrPoHiptdpr9TTq10ugRMX9ZAgzzolhs3kSqdAIAHojZA6Pog3lkwxHjKKmEkNZCLbWAzHpoJCH0QFvlttYPZCUeJVRuTygGdi3NgTJcTvxzX4JRkL3iBsCr5A0QbUZAaPrG7EvCayZBZA2X1znRe54pQsSZAhz7ZBB4nwZDZD";

    private readonly WhatsAppPayloadBuilder _payloadBuilder = new();
    private readonly WhatsAppHTTPClientManager _httpClientHelper;

    public WhatsAppHelper()
    {
        _httpClientHelper = new WhatsAppHTTPClientManager(_accessToken);
    }

    /*public async Task<string> SendTextMessageAsync(tTextMessageDTO req)
    {
        Console.WriteLine($"Sending message to {req.PhoneNumber}: {req.Message}");

        var url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";
        //var payload = _payloadBuilder.BuildTextMessagePayload(req);
        var payload = _payloadBuilder.BuildInteractiveButtonPayload(req);

        return await _httpClientHelper.PostAsync(url , payload);
    }
    public async Task<string> SendImageMessageAsync(tTextMessageDTO req)
    {
        // 1) Upload the photo to get a media ID
        var mediaUrl = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/media";
        var mimeType = "image/jpeg"; // adjust to the actual image format

        req.Photo = File.ReadAllBytes("C:\\Users\\User\\Downloads\\photo_1.jpg");
        var uploadResult = await _httpClientHelper.UploadMediaAsync(mediaUrl , req.Photo , "photo.jpg" , mimeType);

        // Parse the media id from the upload response: { "id": "..." }
        using var doc = JsonDocument.Parse(uploadResult);
        var mediaId = doc.RootElement.GetProperty("id").GetString();

        // 2) Send the image message using that media id
        var messagesUrl = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";
        var payload = _payloadBuilder.BuildImageMessagePayload(req , mediaId);
        return await _httpClientHelper.PostAsync(messagesUrl , payload);
    }*/

    /// <summary>
    /// Sends a message based on the data present in the DTO:
    /// - Has Photo        → sends an image (uploads then sends by media ID)
    /// - Has ButtonList   → sends an interactive button message
    /// - Otherwise        → sends a text message (honors PreviewURL)
    /// </summary>
    public async Task<string> SendMessageAsync(tTextMessageDTO req)
    {
        var messagesUrl = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";
        var mediaUrl = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/media";

#if DEBUG
        //req.Photo ??= File.ReadAllBytes("C:\\Users\\User\\Downloads\\photo_1.jpg");
        //req.Video ??= File.ReadAllBytes("C:\\Users\\User\\Downloads\\Sheikh Muhammad Al Luhaidan emotional recitation of the Quran  #quran #quranrecitation #luhaidan.mp4");
        //req.Audio ??= File.ReadAllBytes("C:\\Users\\User\\Downloads\\Sheikh Muhammad Al Luhaidan emotional recitation of the Quran  #quran #quranrecitation #luhaidan.mp3");
        //req.Audio ??= File.ReadAllBytes("C:\\Users\\User\\Downloads\\WhatsApp Ptt 2026-07-28 at 7.39.09 PM.ogg");
        //req.Document ??= File.ReadAllBytes("C:\\Users\\User\\Downloads\\Tech Ventures - E-Invoicing API Documentation v3.pdf");
        //req.ButtonList ??= new List<tButtonDTO>()
        //{
        //    new tButtonDTO()
        //    {
        //        Type = tButtonDTO.ButtonType.Reply,
        //        Reply = new tReplyButtonDTO()
        //        {
        //            Id = "test_Id_1",
        //            Title = "hard coded button"
        //        }
        //    }
        //};

#endif
        // Local helper: upload raw bytes to WhatsApp and return the media ID.
        async Task<string> UploadAsync(byte[] fileBytes , string fileName , string mimeType)
        {
            var uploadResult = await _httpClientHelper.UploadMediaAsync(mediaUrl , fileBytes , fileName , mimeType);
            using var doc = JsonDocument.Parse(uploadResult);
            return doc.RootElement.GetProperty("id").GetString();
        }

        // Media types need a FileName so the MIME type resolves correctly.
        // ResolveMimeType throws a clear error if it can't determine the type,
        // rather than silently sending application/octet-stream (which WhatsApp
        // would reject with error 131053 for a mismatched MIME type).
        if (!string.IsNullOrWhiteSpace(req.FileName))
        {
            // Document (requires a filename) — highest precedence
            if (req.Document is { Length: > 0 })
            {
                var mimeType = WhatsAppPayloadBuilder.ResolveMimeType(req.MimeType , req.FileName , isDocument: true);
                var mediaId = await UploadAsync(req.Document , req.FileName , mimeType);
                var payload = _payloadBuilder.BuildDocumentMessagePayload(req , mediaId);
                return await _httpClientHelper.PostAsync(messagesUrl , payload);
            }

            // Video — route by uploaded bytes
            if (req.Video is { Length: > 0 })
            {
                var mimeType = WhatsAppPayloadBuilder.ResolveMimeType(req.MimeType , req.FileName , isDocument: false);
                var mediaId = await UploadAsync(req.Video , req.FileName , mimeType);
                var payload = _payloadBuilder.BuildVideoMessagePayload(req , mediaId);
                return await _httpClientHelper.PostAsync(messagesUrl , payload);
            }

            // Audio — route by uploaded bytes
            if (req.Audio is { Length: > 0 })
            {
                var mimeType = WhatsAppPayloadBuilder.ResolveMimeType(req.MimeType , req.FileName , isDocument: false);
                var mediaId = await UploadAsync(req.Audio , req.FileName , mimeType);
                var payload = _payloadBuilder.BuildAudioMessagePayload(req , mediaId);
                return await _httpClientHelper.PostAsync(messagesUrl , payload);
            }
        }

        string photoId = null;
        // Upload the image first if present (used by both image and interactive paths).
        // Defaults to photo.jpg when no FileName is supplied.
        if (req.Photo is { Length: > 0 })
        {
            var photoName = !string.IsNullOrWhiteSpace(req.FileName) ? req.FileName : "photo.jpg";
            var mimeType = WhatsAppPayloadBuilder.ResolveMimeType(req.MimeType , photoName , isDocument: false);
            photoId = await UploadAsync(req.Photo , photoName , mimeType);
        }

        // Explicit interactive layouts — list / cta_url / location
        // (handled before the legacy button fallbacks below)
        if (!string.IsNullOrWhiteSpace(req.InteractiveType))
        {
            return req.InteractiveType.ToLowerInvariant() switch
            {
                "list" => await _httpClientHelper.PostAsync(messagesUrl , _payloadBuilder.BuildInteractiveListPayload(req)) ,
                "cta_url" => await _httpClientHelper.PostAsync(messagesUrl , _payloadBuilder.BuildInteractiveCtaUrlPayload(req)) ,
                "location" => await _httpClientHelper.PostAsync(messagesUrl , _payloadBuilder.BuildInteractiveLocationPayload(req)) ,
                _ => throw new NotSupportedException($"Unsupported InteractiveType '{req.InteractiveType}'.")
            };
        }

        // image + text + buttons  → interactive with image header
        if (photoId != null && req.ButtonList is { Count: > 0 })
        {
            var payload = _payloadBuilder.BuildInteractiveMessagePayload(req , photoId);
            return await _httpClientHelper.PostAsync(messagesUrl , payload);
        }

        // image + text → image message with caption
        if (photoId != null)
        {
            var payload = _payloadBuilder.BuildImageMessagePayload(req , photoId);
            return await _httpClientHelper.PostAsync(messagesUrl , payload);
        }

        // text + buttons → interactive (no header)
        if (req.ButtonList is { Count: > 0 })
        {
            var payload = _payloadBuilder.BuildInteractiveMessagePayload(req);
            return await _httpClientHelper.PostAsync(messagesUrl , payload);
        }

        // plain text
        var textPayload = _payloadBuilder.BuildTextMessagePayload(req);
        return await _httpClientHelper.PostAsync(messagesUrl , textPayload);
    }


    public async Task<string> SendTemplateMessageAsync(
        string to ,
        string templateName ,
        IList<tTemplateParameter> parameterList = null ,
        string languageCode = "en_US")
    {
        var url = $"https://graph.facebook.com/v25.0/{_phoneNumberId}/messages";
        var payload = _payloadBuilder.BuildTemplateMessagePayload(to , templateName , parameterList , languageCode);

        return await _httpClientHelper.PostAsync(url , payload);
    }


    /// <summary>
    /// Fetches every message template provisioned under the configured WhatsApp Business Account.
    /// This is the canonical way to audit your template library — names, statuses, categories,
    /// languages, and component structures are all returned in a single call.
    /// </summary>
    /// <param name="fields">
    /// A comma-separated list of template properties to include in the response.
    /// Defaults to <c>"name,status,category,language,components"</c>.
    /// </param>
    /// <param name="limit">
    /// Maximum number of templates to return per page. The Cloud API caps this at 100;
    /// this method defaults to a conservative 50.
    /// </param>
    /// <param name="ct">A token to cancel the request mid-flight if the caller gives up.</param>
    /// <returns>
    /// The raw JSON response body from the Graph API as a string. Successful responses
    /// contain a <c>"data"</c> array whose elements represent individual template objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the internal WABA ID or access token has not been configured.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the API refuses the request — for example, an expired token (401),
    /// insufficient permissions (403), or a malformed URL (404).
    /// </exception>
    /// <exception cref="TaskCanceledException">
    /// Thrown when the request times out or the caller cancels via <paramref name="ct"/>.
    /// </exception>
    public async Task<WhatsAppTemplateResponse> GetAllTemplatesAsync(
        string fields = "name,status,category,language,components" ,
        int limit = 50 ,
        CancellationToken ct = default)
    {
        // --- Input validation ---
        if (string.IsNullOrWhiteSpace(_WhatsAppBusinessAccountId))
            throw new ArgumentNullException(nameof(_WhatsAppBusinessAccountId) , "WABA ID cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new ArgumentNullException(nameof(_accessToken) , "Access token cannot be null or empty.");

        // --- Build the URL ---
        var url = $"https://graph.facebook.com/v22.0/{_WhatsAppBusinessAccountId}/message_templates"
                + $"?fields={Uri.EscapeDataString(fields)}"
                + $"&limit={limit}";

        string respond = await _httpClientHelper.GetAsync(url);

        var templates = JsonSerializer.Deserialize<WhatsAppTemplateResponse>(respond)
            ?? throw new InvalidOperationException("Failed to deserialize WhatsApp template response.");

        return templates;
    }
}
