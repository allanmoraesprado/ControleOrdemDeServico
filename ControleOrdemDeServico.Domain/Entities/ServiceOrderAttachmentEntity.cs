using OsService.Domain.Enums;

namespace OsService.Domain.Entities;

public sealed class ServiceOrderAttachmentEntity
{
    public Guid Id { get; init; }
    public Guid ServiceOrderId { get; init; }
    public ServiceOrderAttachmentType Type { get; init; }
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long SizeBytes { get; init; }
    public string StoragePath { get; init; } = default!;
    public DateTime UploadedAt { get; init; }
}