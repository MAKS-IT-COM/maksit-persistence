using System.Reflection;


namespace MaksIT.Persistence.Infrastructure;

/// <summary>FluentMigrator startup options. Product hooks stay product-owned.</summary>
public sealed record PostgresMigrationOptions {
  /// <summary>PostgreSQL connection string.</summary>
  public required string ConnectionString { get; init; }

  /// <summary>Assembly that contains <c>[Migration]</c> types.</summary>
  public required Assembly MigrationAssembly { get; init; }

  /// <summary>When true, scan with <c>For.All()</c> (version metadata); otherwise <c>For.Migrations()</c>.</summary>
  public bool ScanAll { get; init; }

  /// <summary>When true, create the application database if it does not exist.</summary>
  public bool EnsureDatabase { get; init; } = true;

  /// <summary>Optional product DDL after MigrateUp (coordination tables).</summary>
  public Func<CancellationToken, Task>? EnsureCoordinationTables { get; init; }

  /// <summary>Optional product verification after coordination DDL.</summary>
  public Func<CancellationToken, Task>? VerifySchema { get; init; }
}