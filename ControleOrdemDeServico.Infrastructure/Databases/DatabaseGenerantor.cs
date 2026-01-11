using Dapper;

namespace OsService.Infrastructure.Databases;

public class DatabaseGenerantor(
    IAdminSqlConnectionFactory adminFactory,
    IDefaultSqlConnectionFactory defaultFactory)
{
    private const string CreateDbSql = @"
        IF DB_ID(N'OsServiceDb') IS NULL
        BEGIN
            CREATE DATABASE OsServiceDb;
        END;";

    private const string CreateTablesSql = """
        IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Customers (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                Name NVARCHAR(150) NOT NULL,
                Phone NVARCHAR(30) NULL,
                Email NVARCHAR(120) NULL,
                Document NVARCHAR(30) NULL,
                CreatedAt DATETIME2 NOT NULL
            );

            CREATE INDEX IX_Customers_Phone ON dbo.Customers(Phone);
            CREATE INDEX IX_Customers_Document ON dbo.Customers(Document);
        END;

        IF OBJECT_ID(N'dbo.ServiceOrders', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.ServiceOrders (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                Number INT IDENTITY(1000, 1) NOT NULL,
                CustomerId UNIQUEIDENTIFIER NOT NULL,
                Description NVARCHAR(500) NOT NULL,
                Status INT NOT NULL,
                OpenedAt DATETIME2 NOT NULL,
                Price DECIMAL(18, 2) NULL,
                Coin VARCHAR(4) NULL,
                UpdatedPriceAt DATETIME2 NULL,
                StartedAt DATETIME2 NULL,
                FinishedAt DATETIME2 NULL,
                CONSTRAINT FK_ServiceOrders_Customers
                    FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
            );

            CREATE UNIQUE INDEX UX_ServiceOrders_Number ON dbo.ServiceOrders(Number);
            CREATE INDEX IX_ServiceOrders_CustomerId ON dbo.ServiceOrders(CustomerId);
        END;

        IF OBJECT_ID(N'dbo.ServiceOrderAttachments', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.ServiceOrderAttachments (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                ServiceOrderId UNIQUEIDENTIFIER NOT NULL,
                Type INT NOT NULL,
                FileName NVARCHAR(260) NOT NULL,
                ContentType NVARCHAR(100) NOT NULL,
                SizeBytes BIGINT NOT NULL,
                StoragePath NVARCHAR(400) NOT NULL,
                UploadedAt DATETIME2 NOT NULL,
                CONSTRAINT FK_ServiceOrderAttachments_ServiceOrders
                    FOREIGN KEY (ServiceOrderId) REFERENCES dbo.ServiceOrders(Id)
            );

            CREATE INDEX IX_ServiceOrderAttachments_ServiceOrderId
                ON dbo.ServiceOrderAttachments(ServiceOrderId);
        END;
        """;

    public async Task EnsureCreatedAsync(CancellationToken ct)
    {
        using (var conn = adminFactory.Create())
        {
            await conn.ExecuteAsync(
                new CommandDefinition(CreateDbSql, cancellationToken: ct));
        }

        using (var conn = defaultFactory.Create())
        {
            await conn.ExecuteAsync(
                new CommandDefinition(CreateTablesSql, cancellationToken: ct));
        }
    }
}