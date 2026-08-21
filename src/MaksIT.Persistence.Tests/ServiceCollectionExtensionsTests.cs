using Microsoft.Extensions.DependencyInjection;
using MaksIT.Persistence.Data;
using MaksIT.Persistence.Extensions;
using MaksIT.Persistence.Infrastructure;


namespace MaksIT.Persistence.Tests;

public class ServiceCollectionExtensionsTests {
  [Fact]
  public void RegisterLinq2Db_RegistersFactoryAndOptions() {
    var services = new ServiceCollection();
    services.AddLogging();
    services.RegisterLinq2Db("Host=localhost;Database=app;Username=u;Password=p");

    var provider = services.BuildServiceProvider();
    var factory = provider.GetRequiredService<ILinq2DbConnectionFactory>();
    var options = provider.GetRequiredService<ILinq2DbConnectionOptions>();

    Assert.NotNull(factory);
    Assert.Contains("Database=app", options.ConnectionString, StringComparison.OrdinalIgnoreCase);
    Assert.Null(options.MappingSchema);

    using var db = factory.Create();
    Assert.NotNull(db);
  }

  [Fact]
  public void RegisterLinq2Db_EmptyConnectionString_Throws() {
    var services = new ServiceCollection();
    Assert.Throws<ArgumentException>(() => services.RegisterLinq2Db(" "));
  }

  [Fact]
  public void RegisterPostgresMigrations_RegistersRunner() {
    var services = new ServiceCollection();
    services.AddLogging();
    services.RegisterPostgresMigrations(
      "Host=localhost;Database=app;Username=u;Password=p",
      typeof(ServiceCollectionExtensionsTests).Assembly);

    Assert.Contains(services, d => d.ServiceType == typeof(IRunMigrationsService));
  }

  [Fact]
  public void RegisterPostgresMigrations_DefaultsEnsureDatabaseTrue() {
    var services = new ServiceCollection();
    services.AddLogging();
    services.RegisterPostgresMigrations(
      "Host=localhost;Database=app;Username=u;Password=p",
      typeof(ServiceCollectionExtensionsTests).Assembly);

    var options = services.BuildServiceProvider().GetRequiredService<PostgresMigrationOptions>();
    Assert.True(options.EnsureDatabase);
  }

  [Fact]
  public void RegisterPostgresMigrations_PreservesEnsureDatabaseFalse() {
    var services = new ServiceCollection();
    services.AddLogging();
    var assembly = typeof(ServiceCollectionExtensionsTests).Assembly;
    services.RegisterPostgresMigrations(
      "Host=localhost;Database=app;Username=u;Password=p",
      assembly,
      new PostgresMigrationOptions {
        ConnectionString = "Host=localhost;Database=app;Username=u;Password=p",
        MigrationAssembly = assembly,
        EnsureDatabase = false,
      });

    var options = services.BuildServiceProvider().GetRequiredService<PostgresMigrationOptions>();
    Assert.False(options.EnsureDatabase);
  }
}