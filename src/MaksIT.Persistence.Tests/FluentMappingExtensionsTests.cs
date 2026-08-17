using LinqToDB.Mapping;
using MaksIT.Persistence.Data;


namespace MaksIT.Persistence.Tests;

public class FluentMappingExtensionsTests {
  [Fact]
  public void MapScalarPropertiesToSnakeCase_IgnoresNavigations() {
    var schema = new MappingSchema();
    var builder = new FluentMappingBuilder(schema);
    builder.Entity<SampleDto>()
      .HasTableName("samples")
      .MapScalarPropertiesToSnakeCase(x => x.Children);
    builder.Build();

    var ed = schema.GetEntityDescriptor(typeof(SampleDto));
    Assert.Equal("samples", ed.Name.Name);
    Assert.Contains(ed.Columns, c => c.ColumnName == "user_id");
    Assert.Contains(ed.Columns, c => c.ColumnName == "id");
    Assert.DoesNotContain(ed.Columns, c => c.MemberName == nameof(SampleDto.Children) && !c.SkipOnEntityFetch);
  }

  public sealed class SampleDto {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<string> Children { get; set; } = [];
  }
}