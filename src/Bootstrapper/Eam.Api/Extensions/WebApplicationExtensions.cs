using Eam.Api.Configuration;
using Eam.Modules.Assets.Presentation;
using Eam.Modules.Identity.Presentation;
using Eam.Modules.Maintenance.Presentation;
using Eam.Modules.Organization.Presentation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace Eam.Api.Extensions;

/// <summary>
/// Middleware pipeline and endpoint mapping for the API host.
/// </summary>
internal static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP middleware pipeline. Order matters: outermost first.
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // 1. Outermost, so the final status code (including handled errors) is logged.
        app.UseHttpLogging();

        // 2. Converts unhandled exceptions into ProblemDetails via GlobalExceptionHandler.
        app.UseExceptionHandler();

        // 3. Transport security. HSTS is never sent from Development (it would pin localhost to HTTPS).
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // 4. ProblemDetails bodies for empty error responses (for example 404 from routing).
        app.UseStatusCodePages();

        // 5. CORS for the separately hosted React frontend.
        app.UseCors(CorsSettings.PolicyName);

        return app;
    }

    /// <summary>
    /// Maps infrastructure endpoints (health, OpenAPI) and every module's API endpoints.
    /// </summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(ApiRoutes.Liveness, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live),
        });

        // API documentation is exposed in Development only.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options.WithTitle("Enterprise Asset Management API"));
        }

        var api = app.MapGroup(ApiRoutes.Prefix);

        api.MapOrganizationEndpoints()
            .MapIdentityEndpoints()
            .MapAssetEndpoints()
            .MapMaintenanceEndpoints();

        return app;
    }
}
