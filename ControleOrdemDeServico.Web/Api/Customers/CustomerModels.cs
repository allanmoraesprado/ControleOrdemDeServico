namespace OsService.Web.Api.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Document);

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Document,
    DateTime CreatedAt);

public sealed record CreateCustomerResult(Guid Id);