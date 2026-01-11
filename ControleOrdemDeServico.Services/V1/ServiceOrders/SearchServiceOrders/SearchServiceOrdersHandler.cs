using MediatR;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.Dtos;

namespace OsService.Services.V1.ServiceOrders.SearchServiceOrders;

public sealed class SearchServiceOrdersHandler(IServiceOrderRepository repo)
    : IRequestHandler<SearchServiceOrdersQuery, IReadOnlyList<ServiceOrderDto>>
{
    public async Task<IReadOnlyList<ServiceOrderDto>> Handle(
        SearchServiceOrdersQuery request,
        CancellationToken ct)
    {
        if (request.CustomerId is null
            && request.Status is null
            && request.From is null
            && request.To is null)
        {
            throw new ValidationException("At least one filter (customerId, status or period) must be provided.");
        }

        var entities = await repo.SearchAsync(
            request.CustomerId,
            request.Status,
            request.From,
            request.To,
            ct);

        return entities
            .Select(ServiceOrderDto.FromEntity)
            .ToList();
    }
}