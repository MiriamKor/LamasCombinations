namespace Combinations.Sessions;

/// <summary>Exclusive access to one session; disposing runs the store's release (save + unlock).</summary>
public sealed class SessionLease : IAsyncDisposable
{
    private SessionData? _session;
    private Func<ValueTask>? _release;

    public SessionLease(SessionData session, Func<ValueTask> release)
    {
        _session = session;
        _release = release;
    }

    public SessionData Session =>
        _session ?? throw new ObjectDisposedException(nameof(SessionLease));

    public async ValueTask DisposeAsync()
    {
        var release = Interlocked.Exchange(ref _release, null);
        _session = null;

        if (release is not null)
            await release();
    }
}
