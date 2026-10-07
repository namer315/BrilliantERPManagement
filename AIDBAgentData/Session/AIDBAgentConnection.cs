using CommonData.Session;

namespace AIDBAgentData.Session;

public class AIDBAgentConnection : Connection
{
    /// <summary>
    /// AI service endpoint URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// API key used to authenticate with the AI service.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;
    ///// <summary>API key for the DeepSeek API ().</summary>
    //public const string ApiKey = "sk-e7b8ba1c4b2c4b7389a333f44c5d15d4";

}
