using MediatR;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.GetServiceOrderById;

public sealed record GetServiceOrderByIdQuery(Guid Id)
    : IRequest<ServiceOrderDto?>;