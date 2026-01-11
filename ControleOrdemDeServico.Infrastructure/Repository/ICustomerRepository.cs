using OsService.Domain.Entities;

namespace OsService.Infrastructure.Repository;

public interface ICustomerRepository
{
    Task<(Guid id, int number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct);

    Task InsertAsync(CustomerEntity customer, CancellationToken ct);
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
