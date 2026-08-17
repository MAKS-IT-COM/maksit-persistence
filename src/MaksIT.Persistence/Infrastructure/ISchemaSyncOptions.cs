namespace MaksIT.Persistence.Infrastructure;

/// <summary>Options for <see cref="ISchemaSyncService"/>.</summary>
public interface ISchemaSyncOptions {
  /// <summary>PostgreSQL connection string.</summary>
  string ConnectionString { get; }

  /// <summary>When false, <see cref="ISchemaSyncService.SyncSchemaAsync"/> is a no-op.</summary>
  bool AutoSync { get; }

  /// <summary>Desired table → columns map (product-owned).</summary>
  IReadOnlyDictionary<string, IReadOnlyList<SchemaColumn>> DesiredSchema { get; }
}