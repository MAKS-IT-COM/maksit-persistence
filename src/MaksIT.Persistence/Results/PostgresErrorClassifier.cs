using System.Net.Sockets;
using Npgsql;


namespace MaksIT.Persistence.Results;

/// <summary>
/// Classifies PostgreSQL / transport exceptions for retry and Result mapping.
/// </summary>
public static class PostgresErrorClassifier {
  /// <summary>Unique violation SQLSTATE.</summary>
  public const string UniqueViolation = "23505";

  /// <summary>True when the failure is a unique constraint violation.</summary>
  public static bool IsUniqueViolation(Exception exception) {
    for (var current = exception; current is not null; current = current.InnerException) {
      if (current is PostgresException pg && pg.SqlState == UniqueViolation)
        return true;

      if (current is NpgsqlException npg && npg.SqlState == UniqueViolation)
        return true;
    }

    return false;
  }

  /// <summary>True when Postgres is not ready yet or the network blipped.</summary>
  public static bool IsTransient(Exception exception) {
    for (var current = exception; current is not null; current = current.InnerException) {
      if (current is TimeoutException or IOException or SocketException)
        return true;

      if (current is NpgsqlException npg) {
        if (npg.SqlState is "57P03" or "53300")
          return true;

        if (string.IsNullOrEmpty(npg.SqlState))
          return true;
      }
    }

    return false;
  }
}