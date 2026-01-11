using MediatR;

namespace OsService.Services.V1.ServiceOrders.OpenServiceOrder;

public sealed record OpenServiceOrderCommand(
    Guid CustomerId,
    string Description,
    decimal? Price
) : IRequest<(Guid Id, int Number)>;