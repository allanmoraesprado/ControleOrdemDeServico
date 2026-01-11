using MediatR;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.Attachments;

public sealed class GetServiceOrderAttachmentsHandler(IServiceOrderAttachmentRepository attachments)
    : IRequestHandler<GetServiceOrderAttachmentsQuery, IReadOnlyList<ServiceOrderAttachmentDto>>
{
    public async Task<IReadOnlyList<ServiceOrderAttachmentDto>> Handle(
        GetServiceOrderAttachmentsQuery request,
        CancellationToken ct)
    {
        if (request.ServiceOrderId == Guid.Empty)
            throw new ValidationException("Service order id is required.");

        var entities = await attachments.GetByServiceOrderIdAsync(request.ServiceOrderId, ct);
        return entities.Select(ServiceOrderAttachmentDto.FromEntity).ToList();
    }
}