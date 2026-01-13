using System.Text.Json.Serialization;

namespace OsService.Web.Api.ServiceOrders;

public sealed record OpenServiceOrderRequest(
    Guid CustomerId,
    string Description,
    decimal? Price);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceOrderStatus
{
    Open = 0,
    InProgress = 1,
    Finished = 2
}

public sealed record ServiceOrderResponse(
    Guid Id,
    int Number,
    Guid CustomerId,
    string Description,
    ServiceOrderStatus Status,
    DateTime OpenedAt,
    decimal? Price,
    string? Coin,
    DateTime? UpdatedPriceAt,
    DateTime? StartedAt,
    DateTime? FinishedAt);

public sealed record OpenServiceOrderResult(Guid Id, int Number);

public sealed record ChangeServiceOrderStatusRequest(ServiceOrderStatus Status);

public sealed record UpdateServiceOrderPriceRequest(decimal Price);

public sealed record ServiceOrderAttachmentResponse(
    Guid Id,
    string Type,
    string FileName,
    long SizeBytes,
    DateTime UploadedAt);