using MediatR;
using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.ChangeStatus;

public sealed class ChangeServiceOrderStatusHandler(IServiceOrderRepository repo)
    : IRequestHandler<ChangeServiceOrderStatusCommand, ServiceOrderDto>
{
    public async Task<ServiceOrderDto> Handle(
        ChangeServiceOrderStatusCommand request,
        CancellationToken ct)
    {
        if (request.Id == Guid.Empty)
            throw new ValidationException("Service order id is required.");

        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            throw new KeyNotFoundException("Service order not found.");

        var current = entity.Status;
        var desired = request.NewStatus;

        if (current == desired)
            return ServiceOrderDto.FromEntity(entity);

        var now = DateTime.UtcNow;
        var startedAt = entity.StartedAt;
        var finishedAt = entity.FinishedAt;

        switch (current)
        {
            case ServiceOrderStatus.Open when desired == ServiceOrderStatus.InProgress:
                if (startedAt is null)
                    startedAt = now;
                break;

            case ServiceOrderStatus.InProgress when desired == ServiceOrderStatus.Finished:
                if (entity.Price is null)
                    throw new ValidationException("Price is required to finish the service order.");

                if (entity.Price < 0)
                    throw new ValidationException("Price cannot be negative.");

                if (startedAt is null)
                    startedAt = now;

                finishedAt = now;
                break;

            case ServiceOrderStatus.Finished:
                throw new ConflictException("Finished service orders cannot change status.");

            case ServiceOrderStatus.Open when desired == ServiceOrderStatus.Finished:
                throw new ConflictException("Service order must be in progress before being finished.");

            default:
                throw new ConflictException("Invalid status transition for this service order.");
        }

        await repo.UpdateStatusAsync(entity.Id, desired, startedAt, finishedAt, ct);

        var updated = await repo.GetByIdAsync(entity.Id, ct)
                      ?? throw new InvalidOperationException("Service order not found after status update.");

        return ServiceOrderDto.FromEntity(updated);
    }
}