using MediatR;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.GetServiceOrderById;

public sealed class GetServiceOrderByIdHandler(IServiceOrderRepository repo)
    : IRequestHandler<GetServiceOrderByIdQuery, ServiceOrderDto?>
{
    public async Task<ServiceOrderDto?> Handle(
        GetServiceOrderByIdQuery request,
        CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        return entity is null ? null : ServiceOrderDto.FromEntity(entity);
    }
}