using System.Net;

namespace Weaviate.Client;

public class ApiResponse<T>
{
    public HttpStatusCode HttpStatusCode { get; set; }
    public T? Result { get; set; }
    public ErrorResponse? Error { get; set; }
    public string? Uri { get; set; }
    public string? HttpMethod { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }

    public bool IsSuccess => (int)HttpStatusCode >= 200 && (int)HttpStatusCode < 300;
}

public class ErrorResponse
{
    public Error[]? Error { get; set; }
}

public class Error
{
    public string Message { get; set; } = string.Empty;
}
