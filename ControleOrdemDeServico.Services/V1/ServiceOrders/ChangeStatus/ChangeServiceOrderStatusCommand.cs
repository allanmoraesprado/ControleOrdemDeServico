using MediatR;
using OsService.Domain.Enums;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.ChangeStatus;

public sealed record ChangeServiceOrderStatusCommand(
    Guid Id,
    ServiceOrderStatus NewStatus
) : IRequest<ServiceOrderDto>;