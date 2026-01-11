using OsService.Domain.Entities;
using OsService.Domain.Enums;

namespace OsService.Services.V1.ServiceOrders.Dtos;

public sealed record ServiceOrderAttachmentDto(
    Guid Id,
    Guid ServiceOrderId,
    ServiceOrderAttachmentType Type,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath,
    DateTime UploadedAt)
{
    public static ServiceOrderAttachmentDto FromEntity(ServiceOrderAttachmentEntity e) =>
        new(
            e.Id,
            e.ServiceOrderId,
            e.Type,
            e.FileName,
            e.ContentType,
            e.SizeBytes,
            e.StoragePath,
            e.UploadedAt
        );
}