namespace OsService.Web.Api.ServiceOrders;

public sealed record OpenServiceOrderRequest(
    Guid CustomerId,
    string Description,
    decimal? Price);

public sealed record ServiceOrderResponse(
    Guid Id,
    int Number,
    Guid CustomerId,
    string Description,
    int Status,
    DateTime OpenedAt,
    decimal? Price,
    string? Coin,
    DateTime? UpdatedPriceAt,
    DateTime? StartedAt,
    DateTime? FinishedAt);

public sealed record OpenServiceOrderResult(Guid Id, int Number);