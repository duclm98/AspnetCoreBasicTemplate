using System.Text.Json;

namespace AspnetCore.Utilities.Exceptions;

public class ErrorDetail
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    public override string ToString()
    {
        if (Data == null)
            return JsonSerializer.Serialize(new
            {
                statusCode = StatusCode,
                message = Message
            });

        return JsonSerializer.Serialize(new
        {
            statusCode = StatusCode,
            message = Message,
            data = Data
        }, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}