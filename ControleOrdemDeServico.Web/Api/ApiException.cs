using System.Net;

namespace OsService.Web.Api
{
    public sealed class ApiError
    {
        public string? Error { get; set; }
    }

    public sealed class ApiException(string message, HttpStatusCode statusCode) : Exception(message)
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
