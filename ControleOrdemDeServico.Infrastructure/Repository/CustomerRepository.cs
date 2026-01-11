using OsService.Domain.Entities;
using OsService.Infrastructure.Databases;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsService.Infrastructure.Repository;


public sealed class CustomerRepository(IDefaultSqlConnectionFactory factory) : ICustomerRepository
{
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

    public async Task<bool> ExistsByDocumentAsync(string document, CancellationToken ct)
    {
        const string sql = "SELECT 1 FROM dbo.Customers WHERE Document = @Document;";
        using var conn = factory.Create();
        var exists = await conn.QueryFirstOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { Document = document }, cancellationToken: ct));
        return exists.HasValue;
    }

    public async Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct)
    {
        const string sql = "SELECT 1 FROM dbo.Customers WHERE Phone = @Phone;";
        using var conn = factory.Create();
        var exists = await conn.QueryFirstOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { Phone = phone }, cancellationToken: ct));
        return exists.HasValue;
    }

    public async Task<CustomerEntity?> GetByPhoneOrDocumentAsync(string? phone, string? document, CancellationToken ct)
    {
        const string sql = @"
            SELECT TOP 1 Id, Name, Phone, Email, Document, CreatedAt
            FROM dbo.Customers
            WHERE (@Phone IS NOT NULL AND Phone = @Phone)
               OR (@Document IS NOT NULL AND Document = @Document);";

        using var conn = factory.Create();
        return await conn.QuerySingleOrDefaultAsync<CustomerEntity>(
            new CommandDefinition(sql, new { Phone = phone, Document = document }, cancellationToken: ct));
    }
}
