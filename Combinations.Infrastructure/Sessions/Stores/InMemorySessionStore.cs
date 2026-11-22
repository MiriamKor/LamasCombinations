using System.Collections.Concurrent;

namespace Combinations.Sessions;

public class InMemorySessionStore : ISessionStore
{
    private sealed class Entry(SessionData data)
    {
        public SessionData Data { get; } = data;
        public SemaphoreSlim Lock { get; } = new(1, 1);
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public async Task<SessionLease> AcquireAsync(
        string sessionId, DateTimeOffset now, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var entry = _entries.GetOrAdd(sessionId, _ => new Entry(new SessionData(now)));

            await entry.Lock.WaitAsync(cancellationToken);

            // The entry may have been removed/replaced while we waited for the lock.
            if (!_entries.TryGetValue(sessionId, out var current) || !ReferenceEquals(entry, current))
            {
                entry.Lock.Release();
                continue;
            }

            if (entry.Data.IsExpired(now, timeout))
            {
                _entries.TryRemove(new KeyValuePair<string, Entry>(sessionId, entry));
                entry.Lock.Release();
                continue;
            }

            return new SessionLease(entry.Data, () =>
            {
                entry.Lock.Release();
                return ValueTask.CompletedTask;
            });
        }
    }

    public async Task RemoveExpiredAsync(
        DateTimeOffset now, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        foreach (var (sessionId, entry) in _entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await entry.Lock.WaitAsync(cancellationToken);
            try
            {
                if (entry.Data.IsExpired(now, timeout))
                    _entries.TryRemove(new KeyValuePair<string, Entry>(sessionId, entry));
            }
            finally
            {
                entry.Lock.Release();
            }
        }
    }
}
