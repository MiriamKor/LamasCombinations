using Combinations.Models.Internal;

namespace Combinations.Sessions;

/// <summary>Serializable session payload. Locking is the store's responsibility.</summary>
public class SessionData(DateTimeOffset createdAt)
{
    public CombinationState State { get; } = new();

    public DateTimeOffset LastAccessedUtc { get; private set; } = createdAt;

    public void UpdateLastActivity(DateTimeOffset currentTime) => LastAccessedUtc = currentTime;

    public bool IsExpired(DateTimeOffset currentTime, TimeSpan timeout) =>
        currentTime - LastAccessedUtc >= timeout;
}
