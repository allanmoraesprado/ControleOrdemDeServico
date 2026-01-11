using MediatR;
using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.ChangePrice;

public sealed class ChangeServiceOrderPriceHandler(IServiceOrderRepository repo)
    : IRequestHandler<ChangeServiceOrderPriceCommand, ServiceOrderDto>
{
    public async Task<ServiceOrderDto> Handle(
        ChangeServiceOrderPriceCommand request,
        CancellationToken ct)
    {
        if (request.Id == Guid.Empty)
            throw new ValidationException("Service order id is required.");

        if (request.Price is < 0)
            throw new ValidationException("Price cannot be negative.");

        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            throw new KeyNotFoundException("Service order not found.");

        if (entity.Status == ServiceOrderStatus.Finished)
            throw new ConflictException("Price cannot be changed after the service order is finished.");

        var now = DateTime.UtcNow;

        await repo.UpdatePriceAsync(
            entity.Id,
            request.Price,
            entity.Coin ?? "BRL",
            request.Price.HasValue ? now : null,
            ct);

        var updated = await repo.GetByIdAsync(entity.Id, ct)
                      ?? throw new InvalidOperationException("Service order not found after price update.");

        return ServiceOrderDto.FromEntity(updated);
    }
}