using MediatR;
using OsService.Services.V1.Customers.Dtos;

namespace OsService.Services.V1.Customers.GetCustomerById
{
    public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;
}
