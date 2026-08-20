using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.DTO.Webhooks;

namespace WhatsAppData.DTO.Common;

public class ErrorDTO
{
    //public string Error { get; set; } = string.Empty;
    public int ErrorCode { get; set; }
    public string Message { get; set; }   
    public string Type { get; set; } = string.Empty;
    public string Details { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int StatusCode { get; set; }

    // Default constructor for deserialization
    public ErrorDTO() { }

    // Constructor to directly map from a custom exception
    //public ErrorDTO(ApiException ex)
    //{
    //    ArgumentNullException.ThrowIfNull(ex);

    //    Error = ex.Message;
    //    Code = ex.Code;
    //    Type = ex.Type.ToString();
    //    Details = ex.Details;
    //    OccurredAtUtc = ex.OccurredAtUtc;
    //    StatusCode = ex.HttpStatusCode;
    //}

    // Static factory method for explicit conversion syntax
    //public static ErrorDTO FromException(ApiException ex) => new(ex);
}
