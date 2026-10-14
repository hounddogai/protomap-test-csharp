using Dapper;
using Grpc.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Acme.Workforce.Platform;

/// <summary>The local deployment uses SQLite; domain repositories depend on this connection factory.</summary>
public sealed class WorkforceDatabase(IConfiguration configuration)
{
    public async Task<SqliteConnection> OpenAsync(CancellationToken token)
    {
        var connection = new SqliteConnection(configuration.GetConnectionString("Workforce") ?? "Data Source=workforce.db");
        try
        {
            await connection.OpenAsync(token);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        await using var connection = await OpenAsync(token);
        await connection.ExecuteAsync(new CommandDefinition("""
            CREATE TABLE IF NOT EXISTS records (
                tenant TEXT NOT NULL, kind TEXT NOT NULL, id TEXT NOT NULL, payload BLOB NOT NULL,
                revision INTEGER NOT NULL DEFAULT 1, PRIMARY KEY (tenant, kind, id)
            );
            CREATE TABLE IF NOT EXISTS actions (
                sequence INTEGER PRIMARY KEY AUTOINCREMENT, tenant TEXT NOT NULL, kind TEXT NOT NULL,
                entity_id TEXT NOT NULL, action TEXT NOT NULL, detail TEXT NOT NULL
            );
            """, cancellationToken: token));
    }
}

public sealed record StoredRecord(string Tenant, string Kind, string Id, byte[] Payload);

public interface IRecordRepository
{
    Task SaveAsync(StoredRecord record, CancellationToken token);
    Task RecordAsync(string tenant, string kind, string id, string action, string detail, CancellationToken token);
}

/// <summary>Persists snapshots and operator actions using parameterized commands and real transactions.</summary>
public sealed class DapperRecordRepository(WorkforceDatabase database) : IRecordRepository
{
    public async Task SaveAsync(StoredRecord record, CancellationToken token)
    {
        await using var connection = await database.OpenAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);
        try
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO records (tenant, kind, id, payload) VALUES (@Tenant, @Kind, @Id, @Payload)
                ON CONFLICT (tenant, kind, id) DO UPDATE SET payload = excluded.payload, revision = revision + 1;
                """, record, transaction, cancellationToken: token));
            await transaction.CommitAsync(token);
        }
        catch (SqliteException error)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Could not persist the record"), error.Message);
        }
    }

    public async Task RecordAsync(string tenant, string kind, string id, string action, string detail,
        CancellationToken token)
    {
        await using var connection = await database.OpenAsync(token);
        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO actions (tenant, kind, entity_id, action, detail) VALUES (@tenant, @kind, @id, @action, @detail);
            """, new { tenant, kind, id, action, detail }, cancellationToken: token));
    }
}
