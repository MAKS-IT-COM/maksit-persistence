using System.Reflection;
using Microsoft.Extensions.Logging;
using Npgsql;
using FluentMigrator;
using FluentMigrator.Runner;


namespace MaksIT.Persistence.Infrastructure;

/// <inheritdoc />
public sealed class RunMigrationsService(
  IMigrationRunner migrationRunner,
  ILogger<RunMigrationsService> logger,
  PostgresMigrationOptions options,
  IDatabaseStartupObserver startupObserver
) : IRunMigrationsService {
  /// <inheritdoc />
  public async Task RunAsync(CancellationToken cancellationToken = default) {
    if (string.IsNullOrWhiteSpace(options.ConnectionString))
      throw new InvalidOperationException(
        "Database connection string is empty. FluentMigrator would run in connectionless/preview mode and never commit DDL.");

    var csb = new NpgsqlConnectionStringBuilder(options.ConnectionString);
    logger.LogInformation(
      "Running database migrations (host={Host}, database={Database})...",
      csb.Host ?? "(default)",
      string.IsNullOrEmpty(csb.Database) ? "(default)" : csb.Database);

    var migrationTypeCount = options.MigrationAssembly.GetTypes()
      .Count(t => t.GetCustomAttribute<MigrationAttribute>(inherit: false) is not null);
    logger.LogInformation(
      "FluentMigrator discovered {MigrationCount} migration type(s) in {Assembly}.",
      migrationTypeCount,
      options.MigrationAssembly.GetName().Name);

    var maintenanceConnectionString = BuildMaintenanceConnectionString(options.ConnectionString);

    await DatabaseStartupPhaseRunner.RunAsync(
      startupObserver,
      DatabaseStartupPhases.PostgresMaintenanceReady,
      ct => PostgresStartupWait.WaitUntilReadyAsync(maintenanceConnectionString, logger, ct),
      cancellationToken).ConfigureAwait(false);

    if (options.EnsureDatabase) {
      await DatabaseStartupPhaseRunner.RunAsync(
        startupObserver,
        DatabaseStartupPhases.DatabaseEnsured,
        EnsureDatabaseExistsAsync,
        cancellationToken).ConfigureAwait(false);
    }

    await DatabaseStartupPhaseRunner.RunAsync(
      startupObserver,
      DatabaseStartupPhases.PostgresApplicationReady,
      ct => PostgresStartupWait.WaitUntilReadyAsync(options.ConnectionString, logger, ct),
      cancellationToken).ConfigureAwait(false);

    await DatabaseStartupPhaseRunner.RunAsync(
      startupObserver,
      DatabaseStartupPhases.MigrationsComplete,
      ct => PostgresStartupWait.MigrateUpWithRetryAsync(migrationRunner, logger, ct),
      cancellationToken).ConfigureAwait(false);

    if (options.EnsureCoordinationTables is not null) {
      await DatabaseStartupPhaseRunner.RunAsync(
        startupObserver,
        DatabaseStartupPhases.CoordinationTablesReady,
        options.EnsureCoordinationTables,
        cancellationToken).ConfigureAwait(false);
    }

    if (options.VerifySchema is not null) {
      await DatabaseStartupPhaseRunner.RunAsync(
        startupObserver,
        DatabaseStartupPhases.SchemaVerified,
        options.VerifySchema,
        cancellationToken).ConfigureAwait(false);
    }

    logger.LogInformation("Database migrations completed.");
  }

  static string BuildMaintenanceConnectionString(string connectionString) {
    var builder = new NpgsqlConnectionStringBuilder(connectionString) {
      Database = "postgres",
    };
    return builder.ConnectionString;
  }

  async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken) {
    var builder = new NpgsqlConnectionStringBuilder(options.ConnectionString);
    var database = builder.Database?.Trim();
    if (string.IsNullOrEmpty(database))
      return;

    var postgresCs = BuildMaintenanceConnectionString(options.ConnectionString);

    await using var conn = new NpgsqlConnection(postgresCs);
    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

    await using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname", conn)) {
      cmd.Parameters.AddWithValue("dbname", database);
      await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
      if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        return;
    }

    logger.LogInformation("Database \"{Database}\" does not exist; creating it.", database);
    var quotedDb = $"\"{database.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    await using (var createCmd = new NpgsqlCommand($"CREATE DATABASE {quotedDb}", conn)) {
      await createCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    logger.LogInformation("Database \"{Database}\" created.", database);
  }
}