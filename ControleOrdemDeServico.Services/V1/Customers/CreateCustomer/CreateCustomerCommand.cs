using MediatR;

namespace OsService.Services.V1.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string? Phone,
    string? Email,
    string? Document
) : IRequest<Guid>;
