using OsService.Domain.Entities;
using OsService.Domain.Enums;

namespace OsService.Infrastructure.Repository;

public interface IServiceOrderRepository
{
    Task<(Guid Id, int Number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct);
    Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    Task UpdateStatusAsync(
        Guid id,
        ServiceOrderStatus status,
        DateTime? startedAt,
        DateTime? finishedAt,
        CancellationToken ct);

    Task<IReadOnlyList<ServiceOrderEntity>> SearchAsync(
        Guid? customerId,
        ServiceOrderStatus? status,
        DateTime? from,
        DateTime? to,
        CancellationToken ct);
}
