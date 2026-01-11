using Dapper;
using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Databases;
using System.Text;

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

    public async Task<IReadOnlyList<ServiceOrderEntity>> SearchAsync(
        Guid? customerId,
        ServiceOrderStatus? status,
        DateTime? from,
        DateTime? to,
        CancellationToken ct)
    {
        var sql = new StringBuilder(@"
            SELECT Id, Number, CustomerId, Description,
                   Status = CAST(Status AS INT),
                   OpenedAt, Price, Coin, UpdatedPriceAt
            FROM dbo.ServiceOrders
            WHERE 1 = 1
            ");

        var parameters = new DynamicParameters();

        if (customerId is not null)
        {
            sql.AppendLine("AND CustomerId = @CustomerId");
            parameters.Add("CustomerId", customerId);
        }

        if (status is not null)
        {
            sql.AppendLine("AND Status = @Status");
            parameters.Add("Status", (int)status.Value);
        }

        if (from is not null)
        {
            sql.AppendLine("AND OpenedAt >= @From");
            parameters.Add("From", from.Value);
        }

        if (to is not null)
        {
            sql.AppendLine("AND OpenedAt <= @To");
            parameters.Add("To", to.Value);
        }

        using var conn = factory.Create();
        var rows = await conn.QueryAsync<ServiceOrderEntity>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct));

        return rows.ToList();
    }

    public async Task UpdateStatusAsync(
        Guid id,
        ServiceOrderStatus status,
        DateTime? startedAt,
        DateTime? finishedAt,
        CancellationToken ct)
    {
        const string sql = @"
            UPDATE dbo.ServiceOrders
            SET Status     = @Status,
                StartedAt  = @StartedAt,
                FinishedAt = @FinishedAt
            WHERE Id = @Id;";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                Status = (int)status,
                StartedAt = startedAt,
                FinishedAt = finishedAt
            },
            cancellationToken: ct));
    }

    public async Task UpdatePriceAsync(
        Guid id,
        decimal? price,
        string coin,
        DateTime? updatedPriceAt,
        CancellationToken ct)
    {
        const string sql = @"
            UPDATE dbo.ServiceOrders
            SET Price          = @Price,
                Coin           = @Coin,
                UpdatedPriceAt = @UpdatedPriceAt
            WHERE Id = @Id;";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                Price = price,
                Coin = coin,
                UpdatedPriceAt = updatedPriceAt
            },
            cancellationToken: ct));
    }
}