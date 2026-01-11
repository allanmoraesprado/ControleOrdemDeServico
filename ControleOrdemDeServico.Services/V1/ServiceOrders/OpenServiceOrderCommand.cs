using MediatR;

namespace OsService.Services.V1.ServiceOrders;

public sealed record OpenServiceOrderCommand(
    Guid CustomerId,
    string Description
) : IRequest<(Guid Id, int Number)>;
