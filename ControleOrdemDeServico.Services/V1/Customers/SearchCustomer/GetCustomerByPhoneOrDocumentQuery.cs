using MediatR;
using OsService.Services.V1.Customers.Dtos;

namespace OsService.Services.V1.Customers.SearchCustomer;

public sealed record GetCustomerByPhoneOrDocumentQuery(
    string? Phone,
    string? Document
) : IRequest<CustomerDto?>;