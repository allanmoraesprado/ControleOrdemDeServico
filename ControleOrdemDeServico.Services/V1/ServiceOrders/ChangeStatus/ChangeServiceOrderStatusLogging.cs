using Microsoft.Extensions.Logging;
using OsService.Domain.Enums;

namespace OsService.Services.V1.ServiceOrders.ChangeStatus;

internal static partial class ChangeServiceOrderStatusLogging
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Information,
        Message = "Service order status changed. Id={ServiceOrderId}, From={OldStatus}, To={NewStatus}")]
    public static partial void ServiceOrderStatusChanged(
        this ILogger logger,
        Guid serviceOrderId,
        ServiceOrderStatus oldStatus,
        ServiceOrderStatus newStatus);
}