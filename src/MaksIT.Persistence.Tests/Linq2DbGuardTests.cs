using Microsoft.Extensions.Logging.Abstractions;
using MaksIT.Persistence.Results;
using MaksIT.Results;


namespace MaksIT.Persistence.Tests;

public class Linq2DbGuardTests {
  [Fact]
  public void Execute_Success_ReturnsActionResult() {
    var result = Linq2DbGuard.Execute(() => Result.Ok(), NullLogger.Instance, "unused");
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Execute_GenericException_ReturnsInternalServerError() {
    var result = Linq2DbGuard.Execute(
      () => throw new InvalidOperationException("db down"),
      NullLogger.Instance,
      "failed");

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Messages ?? [], m => m.Contains("failed", StringComparison.Ordinal));
  }

  [Fact]
  public void Execute_Cancellation_Rethrows() =>
    Assert.Throws<OperationCanceledException>(() =>
      Linq2DbGuard.Execute(
        () => throw new OperationCanceledException(),
        NullLogger.Instance,
        "unused"));
}