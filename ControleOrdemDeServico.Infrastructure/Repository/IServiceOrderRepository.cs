using OsService.Domain.Entities;

namespace OsService.Infrastructure.Repository;

public interface IServiceOrderRepository
{
    Task<(Guid Id, int Number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct);
    Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct);
}
