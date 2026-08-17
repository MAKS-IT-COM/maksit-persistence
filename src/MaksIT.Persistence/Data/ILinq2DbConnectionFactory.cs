using LinqToDB.Data;


namespace MaksIT.Persistence.Data;

/// <summary>
/// Creates a scoped linq2db <see cref="DataConnection"/> for PostgreSQL.
/// </summary>
public interface ILinq2DbConnectionFactory {
  /// <summary>Creates a new <see cref="DataConnection"/>. Dispose after use.</summary>
  DataConnection Create();
}