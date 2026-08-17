namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Optional hooks for reporting database startup phase timing (migrations, schema sync).
/// </summary>
public interface IDatabaseStartupObserver {
  /// <summary>Called when a named startup phase begins.</summary>
  void OnPhaseStarted(string phase);

  /// <summary>Called when a named startup phase completes.</summary>
  void OnPhaseCompleted(string phase, TimeSpan elapsed);

  /// <summary>Called when a named startup phase fails.</summary>
  void OnPhaseFailed(string phase, TimeSpan elapsed, Exception exception);
}

/// <summary>Well-known phase names reported during migrations and schema sync.</summary>
public static class DatabaseStartupPhases {
  /// <summary>Maintenance Postgres (database postgres) accepted connections.</summary>
  public const string PostgresMaintenanceReady = "postgres_maintenance_ready";

  /// <summary>Application database created if missing.</summary>
  public const string DatabaseEnsured = "database_ensured";

  /// <summary>Application Postgres accepted connections.</summary>
  public const string PostgresApplicationReady = "postgres_application_ready";

  /// <summary>FluentMigrator MigrateUp completed.</summary>
  public const string MigrationsComplete = "migrations_complete";

  /// <summary>Product coordination tables provisioned.</summary>
  public const string CoordinationTablesReady = "coordination_tables_ready";

  /// <summary>Product schema verification completed.</summary>
  public const string SchemaVerified = "schema_verified";

  /// <summary>Add-only schema sync completed.</summary>
  public const string SchemaSync = "schema_sync";
}

/// <summary>No-op observer used when the host does not register one.</summary>
public sealed class NoOpDatabaseStartupObserver : IDatabaseStartupObserver {
  /// <inheritdoc />
  public void OnPhaseStarted(string phase) { }

  /// <inheritdoc />
  public void OnPhaseCompleted(string phase, TimeSpan elapsed) { }

  /// <inheritdoc />
  public void OnPhaseFailed(string phase, TimeSpan elapsed, Exception exception) { }
}