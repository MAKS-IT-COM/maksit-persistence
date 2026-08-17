namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Builds add-only ALTER TABLE statements. Does not CREATE or DROP tables.
/// </summary>
public static class SchemaSyncDdlBuilder {
  /// <summary>
  /// Returns ALTER TABLE ADD COLUMN IF NOT EXISTS for columns present in <paramref name="desired"/>
  /// and missing from <paramref name="current"/>. Skips tables that do not exist yet.
  /// </summary>
  public static IReadOnlyList<string> BuildAddOnlyDdl(
    IReadOnlyDictionary<string, IReadOnlyList<SchemaColumn>> desired,
    IReadOnlyDictionary<string, HashSet<string>> current
  ) {
    List<string> ddl = [];

    foreach (var (table, columns) in desired) {
      if (!current.TryGetValue(table, out var existingColumns))
        continue;

      foreach (var column in columns) {
        if (existingColumns.Contains(column.Name))
          continue;

        var nullable = column.Nullable ? "" : " NOT NULL";
        ddl.Add($"ALTER TABLE {Quote(table)} ADD COLUMN IF NOT EXISTS {Quote(column.Name)} {column.PgType}{nullable}");
      }
    }

    return ddl;
  }

  /// <summary>Quotes a PostgreSQL identifier.</summary>
  public static string Quote(string name) => $"\"{name.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}