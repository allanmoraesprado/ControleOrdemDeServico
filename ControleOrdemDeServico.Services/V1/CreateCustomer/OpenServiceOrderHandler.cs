using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using MediatR;

namespace OsService.Services.V1.CreateCustomer;

public sealed class OpenServiceOrderHandler(
    ICustomerRepository customers
) : IRequestHandler<OpenServiceOrderCommand, (Guid Id, int Number)>
{
    public async Task<(Guid Id, int Number)> Handle(OpenServiceOrderCommand request, CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 500)
            throw new ArgumentException("Description is required and must be <= 500 chars.");

        var exists = await customers.ExistsAsync(request.CustomerId, ct);
        if (!exists)
            throw new KeyNotFoundException("Customer not found.");

        var so = new ServiceOrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Description = request.Description.Trim(),
            Status = ServiceOrderStatus.Open,
            OpenedAt = DateTime.UtcNow
        };

        return await customers.InsertAndReturnNumberAsync(so, ct);
    }
}
