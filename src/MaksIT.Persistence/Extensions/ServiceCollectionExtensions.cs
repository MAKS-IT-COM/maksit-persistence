using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LinqToDB.Mapping;
using FluentMigrator.Runner;
using MaksIT.Persistence.Data;
using MaksIT.Persistence.Infrastructure;


namespace MaksIT.Persistence.Extensions;

/// <summary>
/// DI registration helpers for MaksIT.Persistence services.
/// </summary>
public static class ServiceCollectionExtensions {
  /// <summary>
  /// Registers a scoped <see cref="ILinq2DbConnectionFactory"/> for PostgreSQL.
  /// Pass <paramref name="mappingSchema"/> for fluent maps; omit for attribute mapping.
  /// </summary>
  public static void RegisterLinq2Db(
    this IServiceCollection services,
    string connectionString,
    MappingSchema? mappingSchema = null
  ) {
    if (string.IsNullOrWhiteSpace(connectionString))
      throw new ArgumentException("connectionString is required.", nameof(connectionString));

    services.TryAddSingleton<IDatabaseStartupObserver, NoOpDatabaseStartupObserver>();
    services.AddSingleton<ILinq2DbConnectionOptions>(new Linq2DbConnectionOptions {
      ConnectionString = connectionString,
      MappingSchema = mappingSchema,
    });
    services.AddScoped<ILinq2DbConnectionFactory, Linq2DbConnectionFactory>();
  }

  /// <summary>
  /// Registers FluentMigrator Postgres runner and <see cref="IRunMigrationsService"/>.
  /// </summary>
  public static void RegisterPostgresMigrations(
    this IServiceCollection services,
    string connectionString,
    Assembly migrationAssembly,
    PostgresMigrationOptions? options = null
  ) {
    if (string.IsNullOrWhiteSpace(connectionString))
      throw new ArgumentException("connectionString is required.", nameof(connectionString));
    ArgumentNullException.ThrowIfNull(migrationAssembly);

    var resolved = options is null
      ? new PostgresMigrationOptions {
        ConnectionString = connectionString,
        MigrationAssembly = migrationAssembly,
      }
      : options with {
        ConnectionString = string.IsNullOrWhiteSpace(options.ConnectionString) ? connectionString : options.ConnectionString,
        MigrationAssembly = options.MigrationAssembly ?? migrationAssembly,
      };

    services.TryAddSingleton<IDatabaseStartupObserver, NoOpDatabaseStartupObserver>();
    services.AddSingleton(resolved);
    services.AddFluentMigratorCore()
      .ConfigureRunner(rb => {
        rb.AddPostgres()
          .WithGlobalConnectionString(resolved.ConnectionString);
        var scan = rb.ScanIn(resolved.MigrationAssembly);
        if (resolved.ScanAll)
          scan.For.All();
        else
          scan.For.Migrations();
      })
      .AddLogging(lb => lb.AddFluentMigratorConsole());
    services.AddScoped<IRunMigrationsService, RunMigrationsService>();
  }
}