namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Desired column for add-only schema sync.
/// </summary>
/// <param name="Name">PostgreSQL column name (snake_case).</param>
/// <param name="PgType">PostgreSQL type, e.g. uuid, text, boolean.</param>
/// <param name="Nullable">When false, generated ADD COLUMN includes NOT NULL (no default).</param>
public sealed record SchemaColumn(string Name, string PgType, bool Nullable = false);