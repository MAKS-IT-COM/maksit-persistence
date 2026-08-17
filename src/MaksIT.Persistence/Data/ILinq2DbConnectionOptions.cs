using LinqToDB.Mapping;


namespace MaksIT.Persistence.Data;

/// <summary>
/// Connection settings for <see cref="ILinq2DbConnectionFactory"/>. Mapping schema is optional (attribute mapping when null).
/// </summary>
public interface ILinq2DbConnectionOptions {
  /// <summary>PostgreSQL connection string.</summary>
  string ConnectionString { get; }

  /// <summary>Optional fluent mapping schema. Null uses linq2db attribute mapping.</summary>
  MappingSchema? MappingSchema { get; }
}