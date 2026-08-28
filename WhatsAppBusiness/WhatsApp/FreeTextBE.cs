using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.FreeText;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppData.Extensions;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class FreeTextBE : WhatsAppBE
{
    private FreeTextPayloadBuilderBE _payloadBuilder = new FreeTextPayloadBuilderBE();
    private MessageBE _message = new MessageBE();
    private ContactBE _contact = new ContactBE();

    public async Task<ChatMessageDTO> SendTextMessage(TextDTO text)
    {
        MessageVO message = await _message.GetNew(MessageVO.WhatsAppMessageTypes.Text);
        message.Content = text.Message;

        string s = await _message.Persist(message);

        string payload = _payloadBuilder.BuildTextMessagePayload(text);
        MessageResponseDTO messageResponseDTO = await PostAsync<MessageResponseDTO>("messages" , payload);

        //message.Status = messageResponseDTO.Messages[0]?.MessageStatus;
        message.MessageId = messageResponseDTO.Messages[0]?.Id;

        message.Receiver = await _contact.GetContactBy(messageResponseDTO.Contacts[0]?.WaId);
        if (string.IsNullOrEmpty(message.Receiver.PhoneNumber))
            message.Receiver.PhoneNumber = messageResponseDTO.Contacts[0]?.Input;
        //message.UpdatedAt = DateTime.UtcNow;

        s = await _message.Persist(message, true);

        ChatMessageDTO chatMessageDTO = new ChatMessageDTO();
        chatMessageDTO.Id = message.Id;
        chatMessageDTO.MessageId = message.MessageId;
        //chatMessageDTO.Timestamp = message.Timestamp;
        chatMessageDTO.MessageDirection = ChatMessageDTO.MessageDirections.Outgoing;
        chatMessageDTO.Body = message.Content;

        chatMessageDTO.Contact = new WhatsAppData.DTO.Common.ContactDTO();
        chatMessageDTO.Contact.Id = message.Receiver.Id;
        chatMessageDTO.Contact.WaId = message.Receiver.WaId;

        return chatMessageDTO;
    }


    public async Task<ChatMessageDTO> SendServiceMessage(FreeTextDTO freeText)
    {
        MessageVO message = await _message.GetNew(freeText.MessageType);
        message.Content = freeText.Body;

        string payload = null;
        switch (freeText.MessageType)
        {
            case MessageVO.WhatsAppMessageTypes.Text:
            {
                payload = _payloadBuilder.BuildTextMessagePayload(freeText);
            }
            break;
            case MessageVO.WhatsAppMessageTypes.Image:
            {
                message.Media = new MessageMediaVO();
                message.Media.Message = message;
                message.Media.MediaFile = freeText.Image.FileBytes;
                message.Media.FileName = freeText.Image.FileName;
                message.Media.Type = MessageMediaVO.MediaTypes.Image;

                var mimeType = ResolveMimeType(freeText.Image.MimeType , freeText.Image.FileName , isDocument: false);
                freeText.Image.Id = await UploadMediaAsync(freeText.Image.FileBytes , freeText.Image.FileName , mimeType);
                payload = _payloadBuilder.BuildImageMessagePayload(freeText);
            }
            break;
            case MessageVO.WhatsAppMessageTypes.Video:
            {
                message.Media = new MessageMediaVO();
                message.Media.Message = message;
                message.Media.MediaFile = freeText.Video.FileBytes;
                message.Media.FileName = freeText.Video.FileName;
                message.Media.Type = MessageMediaVO.MediaTypes.Video;

                var mimeType = ResolveMimeType(freeText.Video.MimeType , freeText.Video.FileName , isDocument: false);
                freeText.Video.Id = await UploadMediaAsync(freeText.Video.FileBytes , freeText.Video.FileName , mimeType);
                payload = _payloadBuilder.BuildVideoMessagePayload(freeText);
            }
            break;
            case MessageVO.WhatsAppMessageTypes.Audio:
            {
                message.Media = new MessageMediaVO();
                message.Media.Message = message;
                message.Media.MediaFile = freeText.Audio.FileBytes;
                message.Media.FileName = freeText.Audio.FileName;
                message.Media.Type = MessageMediaVO.MediaTypes.Audio;

                var mimeType = ResolveMimeType(freeText.Audio.MimeType , freeText.Audio.FileName , isDocument: false);
                freeText.Audio.Id = await UploadMediaAsync(freeText.Audio.FileBytes , freeText.Audio.FileName , mimeType);
                payload = _payloadBuilder.BuildAudioMessagePayload(freeText);
            }
            break;
            case MessageVO.WhatsAppMessageTypes.Document:
            {
                message.Media = new MessageMediaVO();
                message.Media.Message = message;
                message.Media.MediaFile = freeText.Document.FileBytes;
                message.Media.FileName = freeText.Document.FileName;
                message.Media.Type = MessageMediaVO.MediaTypes.Document;

                var mimeType = ResolveMimeType(freeText.Document.MimeType , freeText.Document.FileName , isDocument: true);
                freeText.Document.Id = await UploadMediaAsync(freeText.Document.FileBytes , freeText.Document.FileName , mimeType);
                payload = _payloadBuilder.BuildDocumentMessagePayload(freeText);
            }
            break;
        }
        string s = await _message.Persist(message);

        MessageResponseDTO messageResponseDTO = await PostAsync<MessageResponseDTO>("messages" , payload);

        message.MessageId = messageResponseDTO.Messages[0]?.Id;

        message.Receiver = await _contact.GetContactBy(messageResponseDTO.Contacts[0]?.WaId);
        if (string.IsNullOrEmpty(message.Receiver.PhoneNumber))
            message.Receiver.PhoneNumber = messageResponseDTO.Contacts[0]?.Input;

        s = await _message.Persist(message , true);
        ChatMessageDTO chatMessageDTO = message.MapTo<ChatMessageDTO>();

        return chatMessageDTO;
    }

    public async Task<ChatMessageDTO> SendDocumentMessage(DocumentDTO document)
    {
        FreeTextDTO freeText = new FreeTextDTO();
        freeText.MessageType = MessageVO.WhatsAppMessageTypes.Document;

        freeText.Phone = document.Phone;
        freeText.Body = document.Body;

        freeText.Document = new FreeTextDocumentDTO();
        freeText.Document.FileBytes = document.FileBytes;
        freeText.Document.FileName = document.FileName;
        freeText.Document.MimeType = document.MimeType;

        return await SendServiceMessage(freeText);
    }

    public async Task<ChatMessageDTO> SendImageMessage(PhotoDTO photo)
    {
        FreeTextDTO freeText = new FreeTextDTO();
        freeText.MessageType = MessageVO.WhatsAppMessageTypes.Image;

        freeText.Phone = photo.Phone;
        freeText.Body = photo.Body;

        freeText.Image = new FreeTextImageDTO();
        freeText.Image.FileBytes = photo.FileBytes;
        freeText.Image.FileName = photo.FileName;
        freeText.Image.MimeType = photo.MimeType;
        freeText.Image.Caption = photo.Caption;

        return await SendServiceMessage(freeText);
    }

    public async Task<ChatMessageDTO> SendVideoMessage(VideoDTO video)
    {
        FreeTextDTO freeText = new FreeTextDTO();
        freeText.MessageType = MessageVO.WhatsAppMessageTypes.Video;

        freeText.Phone = video.Phone;
        freeText.Body = video.Body;

        freeText.Video = new FreeTextVideoDTO();
        freeText.Video.FileBytes = video.FileBytes;
        freeText.Video.FileName = video.FileName;
        freeText.Video.MimeType = video.MimeType;
        freeText.Video.Caption = video.Caption;

        return await SendServiceMessage(freeText);
    }

    public async Task<ChatMessageDTO> SendAudioMessage(AudioDTO audio)
    {
        FreeTextDTO freeText = new FreeTextDTO();
        freeText.MessageType = MessageVO.WhatsAppMessageTypes.Audio;

        freeText.Phone = audio.Phone;
        freeText.Body = audio.Body;

        freeText.Audio = new FreeTextAudioDTO();
        freeText.Audio.FileBytes = audio.FileBytes;
        freeText.Audio.FileName = audio.FileName;
        freeText.Audio.MimeType = audio.MimeType;

        return await SendServiceMessage(freeText);
    }
    // Pick a sensible MIME type when the caller did not supply one.
    // The MIME is inferred from the file extension. If it can't be determined,
    // an exception is thrown so the mismatch fails loudly *before* hitting the
    // WhatsApp API (which would reject it with error 131053 with a confusing
    // "application/octet-stream" type), instead of silently sending a bad type.
    public static string ResolveMimeType(string mimeType , string fileName , bool isDocument)
    {
        if (!string.IsNullOrWhiteSpace(mimeType))
            return mimeType;

        var ext = System.IO.Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

        if (string.IsNullOrEmpty(ext))
            throw new ArgumentException(
                $"Cannot resolve a MIME type: no file extension found in '{fileName}'. " +
                "Provide a matching FileName (and/or MimeType) so the media upload type is correct.");

        if (isDocument)
        {
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                _ => throw new NotSupportedException(
                    $"Unsupported document extension '{ext}'. Add it to ResolveMimeType or supply MimeType explicitly.")
            };
        }

        return ext switch
        {
            ".mp4" => "video/mp4",
            ".3gp" => "video/3gpp",
            ".mp3" => "audio/mpeg",
            ".ogg" or ".oga" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            ".amr" => "audio/amr",
            ".wav" => "audio/wav",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => throw new NotSupportedException(
                $"Unsupported media extension '{ext}'. Add it to ResolveMimeType or supply MimeType explicitly.")
        };
    }
}
