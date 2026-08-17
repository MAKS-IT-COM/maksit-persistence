using Microsoft.Extensions.Logging;
using Npgsql;
using FluentMigrator.Runner;
using MaksIT.Persistence.Results;


namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Waits for PostgreSQL to accept connections, then retries FluentMigrator on transient failures.
/// </summary>
public static class PostgresStartupWait {
  static readonly TimeSpan InitialDelay = TimeSpan.FromMilliseconds(500);
  static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(5);
  const int MaxAttempts = 60;

  /// <summary>Opens the connection and runs SELECT 1 until ready or attempts exhausted.</summary>
  public static async Task WaitUntilReadyAsync(
    string connectionString,
    ILogger logger,
    CancellationToken cancellationToken
  ) {
    var delay = InitialDelay;
    for (var attempt = 1; attempt <= MaxAttempts; attempt++) {
      cancellationToken.ThrowIfCancellationRequested();
      try {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand("SELECT 1", conn);
        await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (attempt > 1)
          logger.LogInformation("PostgreSQL accepted connections after {Attempt} attempt(s).", attempt);
        return;
      }
      catch (Exception ex) when (PostgresErrorClassifier.IsTransient(ex) && attempt < MaxAttempts) {
        logger.LogWarning(
          ex,
          "PostgreSQL not ready (attempt {Attempt}/{MaxAttempts}); retrying in {DelayMs} ms...",
          attempt,
          MaxAttempts,
          (int)delay.TotalMilliseconds);
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, MaxDelay.TotalMilliseconds));
      }
    }

    throw new InvalidOperationException($"PostgreSQL did not become ready after {MaxAttempts} attempts.");
  }

  /// <summary>Retries FluentMigrator MigrateUp on transient Postgres errors.</summary>
  public static async Task MigrateUpWithRetryAsync(
    IMigrationRunner migrationRunner,
    ILogger logger,
    CancellationToken cancellationToken
  ) {
    var delay = InitialDelay;
    Exception? lastError = null;
    for (var attempt = 1; attempt <= MaxAttempts; attempt++) {
      cancellationToken.ThrowIfCancellationRequested();
      try {
        await Task.Run(() => migrationRunner.MigrateUp(), cancellationToken).ConfigureAwait(false);
        if (attempt > 1)
          logger.LogInformation("FluentMigrator MigrateUp succeeded after {Attempt} attempt(s).", attempt);
        return;
      }
      catch (Exception ex) when (PostgresErrorClassifier.IsTransient(ex) && attempt < MaxAttempts) {
        lastError = ex;
        logger.LogWarning(
          ex,
          "MigrateUp failed while PostgreSQL was unavailable (attempt {Attempt}/{MaxAttempts}); retrying in {DelayMs} ms...",
          attempt,
          MaxAttempts,
          (int)delay.TotalMilliseconds);
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, MaxDelay.TotalMilliseconds));
      }
    }

    throw new InvalidOperationException("FluentMigrator MigrateUp did not succeed after waiting for PostgreSQL.", lastError);
  }
}
