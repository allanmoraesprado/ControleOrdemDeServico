using MediatR;
using OsService.Domain.Enums;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.SearchServiceOrders;

public sealed record SearchServiceOrdersQuery(
    Guid? CustomerId,
    ServiceOrderStatus? Status,
    DateTime? From,
    DateTime? To
) : IRequest<IReadOnlyList<ServiceOrderDto>>;