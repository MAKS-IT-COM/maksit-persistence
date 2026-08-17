namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Runs FluentMigrator at startup. Called from the host before hosted services start.
/// </summary>
public interface IRunMigrationsService {
  /// <summary>Waits for Postgres, optionally creates the database, migrates up, then runs product hooks.</summary>
  Task RunAsync(CancellationToken cancellationToken = default);
}