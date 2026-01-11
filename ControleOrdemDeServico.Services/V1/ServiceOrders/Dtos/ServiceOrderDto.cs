using OsService.Domain.Entities;
using OsService.Domain.Enums;

namespace OsService.Services.V1.ServiceOrders.Dtos;

public sealed record ServiceOrderDto(
    Guid Id,
    int Number,
    Guid CustomerId,
    string Description,
    ServiceOrderStatus Status,
    DateTime OpenedAt,
    decimal? Price,
    string Coin,
    DateTime? UpdatedPriceAt,
    DateTime? StartedAt,
    DateTime? FinishedAt)
{
    public static ServiceOrderDto FromEntity(ServiceOrderEntity e) =>
        new(
            e.Id,
            e.Number,
            e.CustomerId,
            e.Description,
            e.Status,
            e.OpenedAt,
            e.Price,
            e.Coin ?? "BRL",
            e.UpdatedPriceAt,
            e.StartedAt,
            e.FinishedAt
        );
}