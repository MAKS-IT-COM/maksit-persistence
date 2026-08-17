using MaksIT.Persistence.Data;


namespace MaksIT.Persistence.Tests;

public class NameConverterTests {
  [Theory]
  [InlineData("UserId", "user_id")]
  [InlineData("Id", "id")]
  [InlineData("APIKey", "a_p_i_key")]
  [InlineData("", "")]
  public void ToSnakeCase_MatchesVaultConvention(string input, string expected) =>
    Assert.Equal(expected, NameConverter.ToSnakeCase(input));
}