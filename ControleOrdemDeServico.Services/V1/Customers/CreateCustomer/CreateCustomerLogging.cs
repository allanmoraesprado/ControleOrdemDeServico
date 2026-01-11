using Microsoft.Extensions.Logging;

namespace OsService.Services.V1.Customers.CreateCustomer;

internal static partial class CreateCustomerLogging
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Customer created. Id={CustomerId}, Name={Name}, Phone={Phone}, Document={Document}")]
    public static partial void CustomerCreated(
        this ILogger logger,
        Guid customerId,
        string? name,
        string? phone,
        string? document);
}