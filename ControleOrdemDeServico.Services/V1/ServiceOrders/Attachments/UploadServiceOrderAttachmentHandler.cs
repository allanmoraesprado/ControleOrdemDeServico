using MediatR;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;

namespace OsService.Services.V1.ServiceOrders.Attachments;

public sealed class UploadServiceOrderAttachmentHandler(
    IServiceOrderRepository serviceOrders,
    IServiceOrderAttachmentRepository attachments)
    : IRequestHandler<UploadServiceOrderAttachmentCommand, Guid>
{
    public async Task<Guid> Handle(
        UploadServiceOrderAttachmentCommand request,
        CancellationToken ct)
    {
        if (request.ServiceOrderId == Guid.Empty)
            throw new ValidationException("Service order id is required.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ValidationException("File name is required.");

        if (string.IsNullOrWhiteSpace(request.ContentType))
            throw new ValidationException("Content type is required.");

        if (request.SizeBytes <= 0)
            throw new ValidationException("File size must be greater than zero.");

        const long maxSize = 5 * 1024 * 1024;

        if (request.SizeBytes > maxSize)
            throw new ValidationException("File exceeds the maximum allowed size of 5MB.");

        var contentType = request.ContentType.ToLowerInvariant();
        if (contentType is not ("image/jpeg" or "image/png"))
            throw new ValidationException("Only JPG and PNG files are allowed.");

        var existingSo = await serviceOrders.GetByIdAsync(request.ServiceOrderId, ct);
        if (existingSo is null)
            throw new KeyNotFoundException("Service order not found.");

        var now = DateTime.UtcNow;

        var entity = new ServiceOrderAttachmentEntity
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = request.ServiceOrderId,
            Type = request.Type,
            FileName = request.FileName,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            StoragePath = request.StoragePath,
            UploadedAt = now
        };

        await attachments.InsertAsync(entity, ct);
        return entity.Id;
    }
}