namespace MaksIT.Persistence.Infrastructure;

/// <inheritdoc />
public sealed class SchemaSyncOptions : ISchemaSyncOptions {
  /// <inheritdoc />
  public required string ConnectionString { get; init; }

  /// <inheritdoc />
  public required bool AutoSync { get; init; }

  /// <inheritdoc />
  public required IReadOnlyDictionary<string, IReadOnlyList<SchemaColumn>> DesiredSchema { get; init; }
}