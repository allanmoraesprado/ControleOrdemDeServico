using OsService.Domain.Entities;

namespace OsService.Infrastructure.Repository;

public interface IServiceOrderAttachmentRepository
{
    Task InsertAsync(ServiceOrderAttachmentEntity attachment, CancellationToken ct);

    Task<IReadOnlyList<ServiceOrderAttachmentEntity>> GetByServiceOrderIdAsync(
        Guid serviceOrderId,
        CancellationToken ct);
}