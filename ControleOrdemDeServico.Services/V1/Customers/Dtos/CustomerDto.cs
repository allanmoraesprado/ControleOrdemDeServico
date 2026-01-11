using OsService.Domain.Entities;

namespace OsService.Services.V1.Customers.Dtos
{
    public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Document,
    DateTime CreatedAt)
    {
        public static CustomerDto FromEntity(CustomerEntity e) =>
            new(e.Id, e.Name, e.Phone, e.Email, e.Document, e.CreatedAt);
    }
}