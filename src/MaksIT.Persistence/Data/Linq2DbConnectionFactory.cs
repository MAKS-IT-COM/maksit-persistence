using LinqToDB;
using LinqToDB.Data;


namespace MaksIT.Persistence.Data;

/// <inheritdoc />
public sealed class Linq2DbConnectionFactory(ILinq2DbConnectionOptions options) : ILinq2DbConnectionFactory {
  /// <inheritdoc />
  public DataConnection Create() {
    var dataOptions = new DataOptions()
      .UseConnectionString(ProviderName.PostgreSQL, options.ConnectionString);

    if (options.MappingSchema is not null)
      dataOptions = dataOptions.UseMappingSchema(options.MappingSchema);

    return new DataConnection(dataOptions);
  }
}