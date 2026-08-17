using System.Diagnostics;


namespace MaksIT.Persistence.Infrastructure;

/// <summary>
/// Runs a startup phase and reports timing to <see cref="IDatabaseStartupObserver"/>.
/// </summary>
public static class DatabaseStartupPhaseRunner {
  /// <summary>Executes <paramref name="action"/> and reports start/complete/fail.</summary>
  public static async Task RunAsync(
    IDatabaseStartupObserver observer,
    string phase,
    Func<CancellationToken, Task> action,
    CancellationToken cancellationToken
  ) {
    observer.OnPhaseStarted(phase);
    var sw = Stopwatch.StartNew();
    try {
      await action(cancellationToken).ConfigureAwait(false);
      observer.OnPhaseCompleted(phase, sw.Elapsed);
    }
    catch (Exception ex) {
      observer.OnPhaseFailed(phase, sw.Elapsed, ex);
      throw;
    }
  }
}