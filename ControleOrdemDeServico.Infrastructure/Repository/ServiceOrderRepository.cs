using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Databases;
using Dapper;

namespace OsService.Infrastructure.Repository;

public sealed class ServiceOrderRepository(IDefaultSqlConnectionFactory factory) : IServiceOrderRepository
{


    public async Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, Number, CustomerId, Description,
       Status = CAST(Status AS INT),
       OpenedAt
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
            OpenedAt = raw.OpenedAt
        };
    }
}
