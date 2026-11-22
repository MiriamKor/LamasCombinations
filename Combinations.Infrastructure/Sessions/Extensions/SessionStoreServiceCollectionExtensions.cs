using Microsoft.Extensions.Configuration;

namespace Combinations.Sessions;

public static class SessionStoreServiceCollectionExtensions
{
    /// <summary>Provider name (config "SessionStore:Provider") -> registration. Add new stores here only.</summary>
    private static readonly Dictionary<string, Action<IServiceCollection, IConfiguration>> Providers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["InMemory"] = (services, _) => services.AddSingleton<ISessionStore, InMemorySessionStore>(),
            // ["Redis"] = (services, config) => services.AddSingleton<ISessionStore, RedisSessionStore>(),
        };

    public static IServiceCollection AddSessionStore(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["SessionStore:Provider"] ?? "InMemory";

        if (!Providers.TryGetValue(provider, out var register))
            throw new InvalidOperationException(
                $"Unsupported SessionStore:Provider '{provider}'. Supported: {string.Join(", ", Providers.Keys)}.");

        register(services, configuration);
        return services;
    }
}
