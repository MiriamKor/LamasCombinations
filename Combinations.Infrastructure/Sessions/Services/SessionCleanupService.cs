namespace Combinations.Sessions;

public class SessionCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<SessionCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval =
        TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var sessionManager =
                    scope.ServiceProvider.GetRequiredService<ISessionManager>();

                await sessionManager.RemoveExpiredSessionsAsync(stoppingToken);

                logger.LogInformation(
                    "Expired sessions cleanup completed.");
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error occurred while cleaning expired sessions.");
            }
        }
    }
}