using MediatR;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using System.Text.RegularExpressions;

namespace OsService.Services.V1.Customers.CreateCustomer;

using OsService.Services.Exceptions;

public sealed class CreateCustomerHandler(ICustomerRepository repo)
    : IRequestHandler<CreateCustomerCommand, Guid>
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 2 or > 150)
            throw new ValidationException("Name is required and must be between 2 and 150 characters.");

        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        if (phone is not null && phone.Length > 30)
            throw new ValidationException("Phone must be at most 30 characters.");

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        if (email is not null)
        {
            if (email.Length > 120)
                throw new ValidationException("Email must be at most 120 characters.");

            if (!EmailRegex.IsMatch(email))
                throw new ValidationException("Email is invalid.");
        }

        var document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim();
        if (document is not null && document.Length > 30)
            throw new ValidationException("Document must be at most 30 characters.");

        if (document is not null)
        {
            var existsByDoc = await repo.ExistsByDocumentAsync(document, ct);
            if (existsByDoc)
                throw new ConflictException("A customer with the same document already exists.");
        }

        if (phone is not null)
        {
            var existsByPhone = await repo.ExistsByPhoneAsync(phone, ct);
            if (existsByPhone)
                throw new ConflictException("A customer with the same phone already exists.");
        }

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            Email = email,
            Document = document,
            CreatedAt = DateTime.UtcNow
        };

        await repo.InsertAsync(customer, ct);
        return customer.Id;
    }
}
