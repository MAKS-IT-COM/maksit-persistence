namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Syncs the database schema: add missing columns only (no DROP).
/// </summary>
public interface ISchemaSyncService {
  /// <summary>Applies add-only DDL when auto-sync is enabled.</summary>
  Task SyncSchemaAsync(CancellationToken cancellationToken = default);
}