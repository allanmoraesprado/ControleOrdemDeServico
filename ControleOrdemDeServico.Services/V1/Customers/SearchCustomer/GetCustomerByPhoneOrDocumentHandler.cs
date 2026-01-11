using MediatR;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.Customers.Dtos;

namespace OsService.Services.V1.Customers.SearchCustomer;

public sealed class GetCustomerByPhoneOrDocumentHandler(ICustomerRepository repo)
    : IRequestHandler<GetCustomerByPhoneOrDocumentQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerByPhoneOrDocumentQuery request, CancellationToken ct)
    {
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim();

        if (phone is null && document is null)
            throw new ArgumentException("At least phone or document must be provided.");

        var entity = await repo.GetByPhoneOrDocumentAsync(phone, document, ct);
        return entity is null ? null : CustomerDto.FromEntity(entity);
    }
}