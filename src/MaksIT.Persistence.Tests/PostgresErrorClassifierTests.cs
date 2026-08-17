using MaksIT.Persistence.Results;


namespace MaksIT.Persistence.Tests;

public class PostgresErrorClassifierTests {
  [Fact]
  public void IsTransient_TimeoutException_IsTrue() =>
    Assert.True(PostgresErrorClassifier.IsTransient(new TimeoutException("slow")));

  [Fact]
  public void IsTransient_InvalidOperation_IsFalse() =>
    Assert.False(PostgresErrorClassifier.IsTransient(new InvalidOperationException("nope")));

  [Fact]
  public void IsUniqueViolation_PlainException_IsFalse() =>
    Assert.False(PostgresErrorClassifier.IsUniqueViolation(new InvalidOperationException("nope")));
}