using LinqToDB.Mapping;


namespace MaksIT.Persistence.Data;

/// <inheritdoc />
public sealed class Linq2DbConnectionOptions : ILinq2DbConnectionOptions {
  /// <inheritdoc />
  public required string ConnectionString { get; init; }

  /// <inheritdoc />
  public MappingSchema? MappingSchema { get; init; }
}