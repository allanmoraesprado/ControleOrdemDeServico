using Microsoft.Extensions.Logging;

namespace OsService.Services.V1.ServiceOrders.OpenServiceOrder;

internal static partial class OpenServiceOrderLogging
{
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Service order opened. Id={ServiceOrderId}, Number={Number}, CustomerId={CustomerId}, Price={Price}")]
    public static partial void ServiceOrderOpened(
        this ILogger logger,
        Guid serviceOrderId,
        int number,
        Guid customerId,
        decimal? price);
}