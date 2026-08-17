using MaksIT.Persistence.Infrastructure;


namespace MaksIT.Persistence.Tests;

public class DatabaseStartupPhaseRunnerTests {
  [Fact]
  public async Task RunAsync_ReportsStartedAndCompleted() {
    var observer = new RecordingObserver();

    await DatabaseStartupPhaseRunner.RunAsync(
      observer,
      "phase-a",
      _ => Task.CompletedTask,
      TestContext.Current.CancellationToken);

    Assert.Equal(["phase-a"], observer.Started);
    Assert.Equal(["phase-a"], observer.Completed);
    Assert.Empty(observer.Failed);
  }

  [Fact]
  public async Task RunAsync_ReportsFailedAndRethrows() {
    var observer = new RecordingObserver();

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      DatabaseStartupPhaseRunner.RunAsync(
        observer,
        "phase-b",
        _ => throw new InvalidOperationException("boom"),
        TestContext.Current.CancellationToken));

    Assert.Equal(["phase-b"], observer.Started);
    Assert.Empty(observer.Completed);
    Assert.Equal(["phase-b"], observer.Failed);
  }

  sealed class RecordingObserver : IDatabaseStartupObserver {
    public List<string> Started { get; } = [];
    public List<string> Completed { get; } = [];
    public List<string> Failed { get; } = [];

    public void OnPhaseStarted(string phase) => Started.Add(phase);
    public void OnPhaseCompleted(string phase, TimeSpan elapsed) => Completed.Add(phase);
    public void OnPhaseFailed(string phase, TimeSpan elapsed, Exception exception) => Failed.Add(phase);
  }
}