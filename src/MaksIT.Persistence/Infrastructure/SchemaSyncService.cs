using Microsoft.Extensions.Logging;
using Npgsql;


namespace MaksIT.Persistence.Infrastructure;

/// <inheritdoc />
public sealed class SchemaSyncService(
  ISchemaSyncOptions options,
  ILogger<SchemaSyncService> logger,
  IDatabaseStartupObserver startupObserver
) : ISchemaSyncService {
  /// <inheritdoc />
  public async Task SyncSchemaAsync(CancellationToken cancellationToken = default) {
    if (!options.AutoSync) {
      startupObserver.OnPhaseStarted(DatabaseStartupPhases.SchemaSync);
      startupObserver.OnPhaseCompleted(DatabaseStartupPhases.SchemaSync, TimeSpan.Zero);
      return;
    }

    await DatabaseStartupPhaseRunner.RunAsync(
      startupObserver,
      DatabaseStartupPhases.SchemaSync,
      RunSyncCoreAsync,
      cancellationToken).ConfigureAwait(false);
  }

  async Task RunSyncCoreAsync(CancellationToken cancellationToken) {
    logger.LogInformation("Schema sync (add-only) starting...");

    var current = await GetCurrentSchemaAsync(cancellationToken).ConfigureAwait(false);
    var ddl = SchemaSyncDdlBuilder.BuildAddOnlyDdl(options.DesiredSchema, current);
    if (ddl.Count == 0) {
      logger.LogInformation("Schema sync: no changes needed.");
      return;
    }

    await using var conn = new NpgsqlConnection(options.ConnectionString);
    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
    await using var tx = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

    try {
      foreach (var sql in ddl) {
        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        logger.LogDebug("Executed: {Sql}", sql);
      }

      await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
      logger.LogInformation("Schema sync completed. Applied {Count} DDL statement(s).", ddl.Count);
    }
    catch {
      await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
      throw;
    }
  }

  async Task<Dictionary<string, HashSet<string>>> GetCurrentSchemaAsync(CancellationToken ct) {
    var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

    await using var conn = new NpgsqlConnection(options.ConnectionString);
    await conn.OpenAsync(ct).ConfigureAwait(false);
    await using var cmd = new NpgsqlCommand("""
      SELECT table_name, column_name
      FROM information_schema.columns
      WHERE table_schema = 'public'
      ORDER BY table_name, ordinal_position
      """, conn);

    await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
    while (await reader.ReadAsync(ct).ConfigureAwait(false)) {
      var table = reader.GetString(0);
      var column = reader.GetString(1);

      if (!result.TryGetValue(table, out var columns))
        result[table] = columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      columns.Add(column);
    }

    return result;
  }
}