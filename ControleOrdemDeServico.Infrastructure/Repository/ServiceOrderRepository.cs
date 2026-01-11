using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Databases;
using Dapper;

namespace OsService.Infrastructure.Repository;

public sealed class ServiceOrderRepository(IDefaultSqlConnectionFactory factory)
    : IServiceOrderRepository
{
    public async Task<(Guid Id, int Number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO dbo.ServiceOrders (Id, CustomerId, Description, Status, OpenedAt, Price, Coin)
            OUTPUT INSERTED.Id, INSERTED.Number
            VALUES (@Id, @CustomerId, @Description, @Status, @OpenedAt, @Price, @Coin);";

        using var conn = factory.Create();
        var row = await conn.QuerySingleAsync<(Guid Id, int Number)>(
            new CommandDefinition(sql, new
            {
                so.Id,
                so.CustomerId,
                so.Description,
                Status = (int)so.Status,
                so.OpenedAt,
                so.Price,
                so.Coin
            }, cancellationToken: ct));

        return (row.Id, row.Number);
    }

    public async Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id, Number, CustomerId, Description,
                   Status = CAST(Status AS INT),
                   OpenedAt, Price, Coin, UpdatedPriceAt
            FROM dbo.ServiceOrders
            WHERE Id = @Id;";

        using var conn = factory.Create();
        var raw = await conn.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        if (raw is null) return null;

        return new ServiceOrderEntity
        {
            Id = raw.Id,
            Number = raw.Number,
            CustomerId = raw.CustomerId,
            Description = raw.Description,
            Status = (ServiceOrderStatus)(int)raw.Status,
            OpenedAt = raw.OpenedAt,
            Price = raw.Price,
            Coin = raw.Coin,
            UpdatedPriceAt = raw.UpdatedPriceAt
        };
    }
}
