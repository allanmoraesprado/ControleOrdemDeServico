using MediatR;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.ChangePrice;

public sealed record ChangeServiceOrderPriceCommand(
    Guid Id,
    decimal? Price
) : IRequest<ServiceOrderDto>;