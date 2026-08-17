using Microsoft.Extensions.DependencyInjection;
using MaksIT.Persistence.Infrastructure;


namespace MaksIT.Persistence.Extensions;

/// <summary>
/// Host startup helpers for migrations and schema sync.
/// </summary>
public static class ServiceProviderExtensions {
  /// <summary>
  /// Runs <see cref="IRunMigrationsService"/> then <see cref="ISchemaSyncService"/> when registered.
  /// Call from Program.cs before <c>RunAsync</c>.
  /// </summary>
  public static async Task EnsureLinq2DbMigratedAsync(
    this IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default
  ) {
    await using var scope = serviceProvider.CreateAsyncScope();
    var sp = scope.ServiceProvider;

    var run = sp.GetService<IRunMigrationsService>();
    if (run is not null)
      await run.RunAsync(cancellationToken).ConfigureAwait(false);

    var schemaSync = sp.GetService<ISchemaSyncService>();
    if (schemaSync is not null)
      await schemaSync.SyncSchemaAsync(cancellationToken).ConfigureAwait(false);
  }
}