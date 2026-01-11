using MediatR;
using OsService.Domain.Enums;

namespace OsService.Services.V1.ServiceOrders.Attachments;

public sealed record UploadServiceOrderAttachmentCommand(
    Guid ServiceOrderId,
    ServiceOrderAttachmentType Type,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath
) : IRequest<Guid>;