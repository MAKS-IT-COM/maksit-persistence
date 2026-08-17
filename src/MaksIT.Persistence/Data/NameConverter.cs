using System.Text.RegularExpressions;


namespace MaksIT.Persistence.Data;

/// <summary>
/// PascalCase to snake_case helpers for linq2db fluent mapping.
/// </summary>
public static partial class NameConverter {
  /// <summary>Converts <c>UserId</c> to <c>user_id</c>.</summary>
  public static string ToSnakeCase(string input) {
    if (string.IsNullOrEmpty(input))
      return input;

    return SnakeCaseRegex().Replace(input, "_$1").TrimStart('_').ToLowerInvariant();
  }

  [GeneratedRegex(@"([A-Z])", RegexOptions.CultureInvariant)]
  private static partial Regex SnakeCaseRegex();
}