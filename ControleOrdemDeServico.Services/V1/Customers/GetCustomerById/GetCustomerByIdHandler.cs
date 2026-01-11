using MediatR;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.Customers.Dtos;

namespace OsService.Services.V1.Customers.GetCustomerById;

public sealed class GetCustomerByIdHandler(ICustomerRepository repo)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        return entity is null ? null : CustomerDto.FromEntity(entity);
    }
}