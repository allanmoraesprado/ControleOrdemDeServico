using Dapper;
using OsService.Domain.Entities;
using OsService.Infrastructure.Databases;

namespace OsService.Infrastructure.Repository;

public sealed class ServiceOrderAttachmentRepository(IDefaultSqlConnectionFactory defaultFactory) : IServiceOrderAttachmentRepository
{
    private const string InsertSql = """
        INSERT INTO dbo.ServiceOrderAttachments (
            Id,
            ServiceOrderId,
            Type,
            FileName,
            ContentType,
            SizeBytes,
            StoragePath,
            UploadedAt
        ) VALUES (
            @Id,
            @ServiceOrderId,
            @Type,
            @FileName,
            @ContentType,
            @SizeBytes,
            @StoragePath,
            @UploadedAt
        );
        """;

    public async Task InsertAsync(ServiceOrderAttachmentEntity attachment, CancellationToken ct)
    {
        using var conn = defaultFactory.Create();

        await conn.ExecuteAsync(new CommandDefinition(
            InsertSql,
            new
            {
                attachment.Id,
                attachment.ServiceOrderId,
                attachment.Type,
                attachment.FileName,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.StoragePath,
                attachment.UploadedAt
            },
            cancellationToken: ct));
    }

    private const string GetByServiceOrderIdSql = """
        SELECT
            Id,
            ServiceOrderId,
            Type,
            FileName,
            ContentType,
            SizeBytes,
            StoragePath,
            UploadedAt
        FROM dbo.ServiceOrderAttachments
        WHERE ServiceOrderId = @ServiceOrderId
        ORDER BY UploadedAt;
        """;

    public async Task<IReadOnlyList<ServiceOrderAttachmentEntity>> GetByServiceOrderIdAsync(
        Guid serviceOrderId,
        CancellationToken ct)
    {
        using var conn = defaultFactory.Create();

        var result = await conn.QueryAsync<ServiceOrderAttachmentEntity>(
            new CommandDefinition(
                GetByServiceOrderIdSql,
                new { ServiceOrderId = serviceOrderId },
                cancellationToken: ct));

        return result.AsList();
    }
}