using OsService.Domain.Entities;
using OsService.Infrastructure.Databases;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsService.Infrastructure.Repository;


public sealed class CustomerRepository(IDefaultSqlConnectionFactory factory) : ICustomerRepository
{
    public async Task<(Guid id, int number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.ServiceOrders (Id, CustomerId, Description, Status, OpenedAt,Price,Coin)
OUTPUT INSERTED.Id, INSERTED.Number
VALUES (@Id, @CustomerId, @Description, @Status, @OpenedAt,@Price,@Coin);";

        using var conn = factory.Create();
        var row = await conn.QuerySingleAsync<(Guid Id, int Number)>(
            new CommandDefinition(sql, new
            {
                so.Id,
                so.CustomerId,
                so.Description,
                Status = (int)so.Status,
                so.OpenedAt
            }, cancellationToken: ct));

        return (row.Id, row.Number);
    }
    public async Task InsertAsync(CustomerEntity customer, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.Customers (Id, Name, Phone, Email, Document, CreatedAt)
VALUES (@Id, @Name, @Phone, @Email, @Document, @CreatedAt);";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, customer, cancellationToken: ct));
    }

    public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, Name, Phone, Email, Document, CreatedAt
FROM dbo.Customers
WHERE Id = @Id;";

        using var conn = factory.Create();
        return await conn.QuerySingleOrDefaultAsync<CustomerEntity>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT 1 FROM dbo.Customers WHERE Id = @Id;";
        using var conn = factory.Create();
        var exists = await conn.QueryFirstOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return exists.HasValue;
    }
}
