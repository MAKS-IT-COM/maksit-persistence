using MaksIT.Persistence.Infrastructure;


namespace MaksIT.Persistence.Tests;

public class SchemaSyncDdlBuilderTests {
  [Fact]
  public void BuildAddOnlyDdl_AddsMissingColumns_SkipsExistingAndMissingTables() {
    var desired = new Dictionary<string, IReadOnlyList<SchemaColumn>>(StringComparer.OrdinalIgnoreCase) {
      ["users"] = [
        new SchemaColumn("id", "uuid"),
        new SchemaColumn("email", "text", Nullable: true),
        new SchemaColumn("is_active", "boolean"),
      ],
      ["ghost"] = [new SchemaColumn("id", "uuid")],
    };
    var current = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase) {
      ["users"] = new HashSet<string>(["id"], StringComparer.OrdinalIgnoreCase),
    };

    var ddl = SchemaSyncDdlBuilder.BuildAddOnlyDdl(desired, current);

    Assert.Equal(2, ddl.Count);
    Assert.Contains(ddl, s => s.Contains("email", StringComparison.Ordinal) && !s.Contains("NOT NULL", StringComparison.Ordinal));
    Assert.Contains(ddl, s => s.Contains("is_active", StringComparison.Ordinal) && s.Contains("NOT NULL", StringComparison.Ordinal));
    Assert.DoesNotContain(ddl, s => s.Contains("ghost", StringComparison.Ordinal));
  }

  [Fact]
  public void Quote_EscapesEmbeddedQuotes() =>
    Assert.Equal("\"a\"\"b\"", SchemaSyncDdlBuilder.Quote("a\"b"));
}