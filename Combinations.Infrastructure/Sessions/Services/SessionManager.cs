
namespace Combinations.Sessions;

public interface ISessionManager
{
    Task<SessionLease> GetSessionAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default);
    Task RemoveExpiredSessionsAsync(
    CancellationToken cancellationToken = default);
}

public class SessionManager(ISessionStore store, TimeProvider timeProvider) : ISessionManager
{
    private const string SessionIdCookie = "CombinationsSessionId";

    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);

    public async Task<SessionLease> GetSessionAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        string sessionId = GetOrCreateSessionId(httpContext);
        DateTimeOffset now = timeProvider.GetUtcNow();

        var lease = await store.AcquireAsync(sessionId, now, SessionTimeout, cancellationToken);
        lease.Session.UpdateLastActivity(now);

        RefreshCookie(httpContext, sessionId, now);
        return lease;
    }

    public Task RemoveExpiredSessionsAsync(CancellationToken cancellationToken = default) =>
        store.RemoveExpiredAsync(timeProvider.GetUtcNow(), SessionTimeout, cancellationToken);

    private static string GetOrCreateSessionId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(SessionIdCookie, out var sessionId)
            && Guid.TryParse(sessionId, out _))
        {
            return sessionId;
        }

        return Guid.NewGuid().ToString();
    }

    private static void RefreshCookie(HttpContext httpContext, string sessionId, DateTimeOffset now)
    {
        httpContext.Response.Cookies.Append(
            SessionIdCookie,
            sessionId,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = now.Add(SessionTimeout),
                IsEssential = true,
                Path = "/"
            });
    }
}
