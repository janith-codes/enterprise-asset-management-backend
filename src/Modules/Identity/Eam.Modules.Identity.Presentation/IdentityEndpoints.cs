using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Eam.Modules.Identity.Presentation;

/// <summary>
/// HTTP endpoint registration for the Identity module.
/// </summary>
public static class IdentityEndpoints
{
    /// <summary>Route segment for this module, relative to the versioned API group (for example <c>/api/v1</c>).</summary>
    public const string RoutePrefix = "/identity";

    /// <summary>OpenAPI tag used to group this module's operations.</summary>
    public const string Tag = "Identity";

    /// <summary>
    /// Maps the Identity module's endpoints under <see cref="RoutePrefix"/>.
    /// </summary>
    /// <param name="endpoints">The versioned API route group supplied by the host.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // The module's route group. Endpoints are added to it in later milestones.
        _ = endpoints.MapGroup(RoutePrefix).WithTags(Tag);

        return endpoints;
    }
}
