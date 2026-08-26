using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WhatsAppData.DTO.FreeText;
using WhatsAppData.DTO.WhatsApp.FreeText;

namespace WhatsAppBusiness.WhatsApp;

internal class FreeTextPayloadBuilderBE
{
    public string BuildTextMessagePayload(TextDTO text)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = text.PhoneNumber ,
            type = "text" ,
            text = new
            {
                preview_url = text.PreviewURL ,
                body = text.Message
            }
        };

        return JsonSerializer.Serialize(payload);
    }
    public string BuildTextMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "text" ,
            text = new
            {
                preview_url = freeText.Text.PreviewURL ,
                body = freeText.Body
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    public string BuildDocumentMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "document" ,
            document = new
            {
                id = freeText.Document.Id ,
                filename = freeText.Document.FileName ,
                caption = freeText.Body
            }
        };
        return JsonSerializer.Serialize(payload);
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
