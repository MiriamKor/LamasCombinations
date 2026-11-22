namespace Combinations.Sessions;

public interface ICombinationSession
{
    Task<SessionLease> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}
public class CombinationSession(
    ISessionManager sessionManager,
    IHttpContextAccessor httpContextAccessor) : ICombinationSession
{
    public Task<SessionLease> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "No active HTTP context");

        return sessionManager.GetSessionAsync(httpContext, cancellationToken);
    }
}