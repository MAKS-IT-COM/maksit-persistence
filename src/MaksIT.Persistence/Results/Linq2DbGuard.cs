using Microsoft.Extensions.Logging;
using MaksIT.Core.Extensions;
using MaksIT.Results;


namespace MaksIT.Persistence.Results;

/// <summary>
/// Maps linq2db / Postgres exceptions to <see cref="Result"/> / <see cref="Result{T}"/>.
/// Rethrows <see cref="OperationCanceledException"/>.
/// </summary>
public static class Linq2DbGuard {
  /// <summary>Runs <paramref name="action"/> and maps failures.</summary>
  public static Result Execute(Func<Result> action, ILogger logger, string errorMessage) {
    try {
      return action();
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) when (PostgresErrorClassifier.IsUniqueViolation(ex)) {
      if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning(ex, "{ErrorMessage}", errorMessage);

      return Result.Conflict([errorMessage, .. ex.ExtractMessages()]);
    }
    catch (Exception ex) {
      if (logger.IsEnabled(LogLevel.Error))
        logger.LogError(ex, "{ErrorMessage}", errorMessage);

      return Result.InternalServerError([errorMessage, .. ex.ExtractMessages()]);
    }
  }

  /// <summary>Runs <paramref name="action"/> and maps failures.</summary>
  public static Result<T> Execute<T>(Func<Result<T>> action, ILogger logger, string errorMessage) {
    try {
      return action();
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) when (PostgresErrorClassifier.IsUniqueViolation(ex)) {
      if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning(ex, "{ErrorMessage}", errorMessage);

      return Result<T>.Conflict(default, [errorMessage, .. ex.ExtractMessages()]);
    }
    catch (Exception ex) {
      if (logger.IsEnabled(LogLevel.Error))
        logger.LogError(ex, "{ErrorMessage}", errorMessage);

      return Result<T>.InternalServerError(default, [errorMessage, .. ex.ExtractMessages()]);
    }
  }

  /// <summary>Runs <paramref name="action"/> and maps failures.</summary>
  public static async Task<Result> ExecuteAsync(Func<Task<Result>> action, ILogger logger, string errorMessage) {
    try {
      return await action().ConfigureAwait(false);
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) when (PostgresErrorClassifier.IsUniqueViolation(ex)) {
      if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning(ex, "{ErrorMessage}", errorMessage);

      return Result.Conflict([errorMessage, .. ex.ExtractMessages()]);
    }
    catch (Exception ex) {
      if (logger.IsEnabled(LogLevel.Error))
        logger.LogError(ex, "{ErrorMessage}", errorMessage);

      return Result.InternalServerError([errorMessage, .. ex.ExtractMessages()]);
    }
  }

  /// <summary>Runs <paramref name="action"/> and maps failures.</summary>
  public static async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> action, ILogger logger, string errorMessage) {
    try {
      return await action().ConfigureAwait(false);
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) when (PostgresErrorClassifier.IsUniqueViolation(ex)) {
      if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning(ex, "{ErrorMessage}", errorMessage);

      return Result<T>.Conflict(default, [errorMessage, .. ex.ExtractMessages()]);
    }
    catch (Exception ex) {
      if (logger.IsEnabled(LogLevel.Error))
        logger.LogError(ex, "{ErrorMessage}", errorMessage);

      return Result<T>.InternalServerError(default, [errorMessage, .. ex.ExtractMessages()]);
    }
  }
}