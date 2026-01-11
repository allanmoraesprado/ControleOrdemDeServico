using OsService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsService.Domain.Entities;

public sealed class ServiceOrderEntity
{
    public Guid Id { get; init; }
    public int Number { get; init; } 
    public Guid CustomerId { get; init; }
    public string Description { get; init; } = default!;
    public ServiceOrderStatus Status { get; init; }
    public DateTime OpenedAt { get; init; }
    public decimal? Price { get; init; }
    public string? Coin { get; init; } = "BRL";//"USD", "EUR", "BRL", etc.
    public DateTime? UpdatedPriceAt { get; init; }

}
