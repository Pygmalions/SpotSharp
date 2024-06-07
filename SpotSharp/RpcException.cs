using Google.Protobuf;
using Newtonsoft.Json;

namespace SpotSharp;

public class RpcException(string message, IMessage response, Exception? innerException = null)
    : Exception(message + "\n" + response, innerException)
{
    public readonly IMessage Response = response;
}

public static class MessageToDiagnosticJson
{
    private static readonly JsonFormatter Formatter = new JsonFormatter(
        JsonFormatter.Settings.Default.WithIndentation());
    
    public static string ToDiagnosticJson(this IMessage message)
    {
        return JsonConvert.SerializeObject(message, Formatting.Indented);
    }
}