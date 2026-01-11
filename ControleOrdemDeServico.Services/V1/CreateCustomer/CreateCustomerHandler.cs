using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using MediatR;

namespace OsService.Services.V1.CreateCustomer;

public sealed class CreateCustomerHandler(ICustomerRepository repo)
    : IRequestHandler<CreateCustomerCommand, Guid>
{
    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Document = request.Document?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await repo.InsertAsync(customer, ct);
        return customer.Id;
    }
}
