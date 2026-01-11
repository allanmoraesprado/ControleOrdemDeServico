using MediatR;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.Attachments;

public sealed record GetServiceOrderAttachmentsQuery(Guid ServiceOrderId)
    : IRequest<IReadOnlyList<ServiceOrderAttachmentDto>>;