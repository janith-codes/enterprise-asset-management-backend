using Microsoft.Extensions.DependencyInjection;

namespace Eam.Modules.Identity.Infrastructure;

/// <summary>
/// Service registration entry point for the Identity module.
/// The API host (composition root) calls this once at startup.
/// </summary>
public static class IdentityModule
{
    /// <summary>
    /// Registers the services owned by the Identity module.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Persistence, use-case handlers and integration-event consumers for this module
        // are registered here in later milestones.
        return services;
    }
}
