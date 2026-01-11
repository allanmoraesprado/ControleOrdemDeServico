using MediatR;
using Microsoft.Extensions.Logging;
using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;

namespace OsService.Services.V1.ServiceOrders.OpenServiceOrder;

public sealed class OpenServiceOrderHandler(
    ICustomerRepository customers,
    IServiceOrderRepository serviceOrders,
    ILogger<OpenServiceOrderHandler> logger)
    : IRequestHandler<OpenServiceOrderCommand, (Guid Id, int Number)>
{
    public async Task<(Guid Id, int Number)> Handle(OpenServiceOrderCommand request, CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
            throw new ValidationException("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 500)
            throw new ValidationException("Description is required and must be <= 500 chars.");

        if (request.Price is < 0)
            throw new ValidationException("Price cannot be negative.");

        var exists = await customers.ExistsAsync(request.CustomerId, ct);
        if (!exists)
            throw new KeyNotFoundException("Customer not found.");

        var now = DateTime.UtcNow;

        var so = new ServiceOrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Description = request.Description.Trim(),
            Status = ServiceOrderStatus.Open,
            OpenedAt = now,
            Price = request.Price,
            Coin = "BRL",
            UpdatedPriceAt = request.Price.HasValue ? now : null
        };

        var result = await serviceOrders.InsertAndReturnNumberAsync(so, ct);

        logger.ServiceOrderOpened(
            result.Id,
            result.Number,
            request.CustomerId,
            so.Price);

        return result;
    }
}