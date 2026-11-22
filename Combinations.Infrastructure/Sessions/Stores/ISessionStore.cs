namespace Combinations.Sessions;

/// <summary>
/// Persistence + per-session mutual exclusion. This is the single seam to replace for Redis:
/// implement it with a distributed lock, load the state in AcquireAsync and save it in the
/// lease's release callback.
/// </summary>
public interface ISessionStore
{
    /// <summary>
    /// Gets exclusive access to the session, creating it if missing or expired.
    /// Disposing the returned lease persists the state and releases the lock.
    /// </summary>
    Task<SessionLease> AcquireAsync(
        string sessionId, DateTimeOffset now, TimeSpan timeout,
        CancellationToken cancellationToken = default);

    Task RemoveExpiredAsync(
        DateTimeOffset now, TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
