using Microsoft.Extensions.DependencyInjection;

namespace Eam.Modules.Maintenance.Infrastructure;

/// <summary>
/// Service registration entry point for the Maintenance module.
/// The API host (composition root) calls this once at startup.
/// </summary>
public static class MaintenanceModule
{
    /// <summary>
    /// Registers the services owned by the Maintenance module.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddMaintenanceModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Persistence, use-case handlers and integration-event consumers for this module
        // are registered here in later milestones.
        return services;
    }
}
