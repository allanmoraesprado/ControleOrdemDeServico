using OsService.Domain.Entities;

namespace OsService.Infrastructure.Repository;

public interface ICustomerRepository
{
    Task InsertAsync(CustomerEntity customer, CancellationToken ct);
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByDocumentAsync(string document, CancellationToken ct);
    Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct);
    Task<CustomerEntity?> GetByPhoneOrDocumentAsync(string? phone, string? document, CancellationToken ct);
}