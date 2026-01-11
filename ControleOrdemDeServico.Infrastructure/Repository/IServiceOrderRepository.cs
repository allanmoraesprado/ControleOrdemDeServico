using OsService.Domain.Entities;

namespace OsService.Infrastructure.Repository;

public interface IServiceOrderRepository
{
    Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct);
}
