using System.Diagnostics;
using Eam.Api.Configuration;
using Eam.Api.Middleware;
using Eam.Modules.Assets.Infrastructure;
using Eam.Modules.Identity.Infrastructure;
using Eam.Modules.Maintenance.Infrastructure;
using Eam.Modules.Organization.Infrastructure;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Eam.Api.Extensions;

/// <summary>
/// Service registrations for the API host. Keeps <c>Program.cs</c> limited to composition.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers host-level (cross-cutting) services: error handling, request logging,
    /// OpenAPI, health checks and CORS.
    /// </summary>
    public static IServiceCollection AddApiHost(this IServiceCollection services)
    {
        services.AddApiErrorHandling();
        services.AddApiRequestLogging();
        services.AddApiOpenApi();
        services.AddApiHealthChecks();
        services.AddApiCors();

        return services;
    }

    /// <summary>
    /// Registers every business module. Each module owns its own registrations.
    /// </summary>
    public static IServiceCollection AddModules(this IServiceCollection services) =>
        services
            .AddOrganizationModule()
            .AddIdentityModule()
            .AddAssetsModule()
            .AddMaintenanceModule();

    private static void AddApiErrorHandling(this IServiceCollection services)
    {
        // RFC 9457 ProblemDetails for every error response, including empty 4xx/5xx
        // responses produced by routing (via UseStatusCodePages).
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var httpContext = context.HttpContext;

            context.ProblemDetails.Instance ??= $"{httpContext.Request.Method} {httpContext.Request.Path}";
            context.ProblemDetails.Extensions.TryAdd(
                "traceId",
                Activity.Current?.Id ?? httpContext.TraceIdentifier);
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
    }

    private static void AddApiRequestLogging(this IServiceCollection services) =>
        // Built-in HTTP logging. Headers, query strings and bodies are deliberately excluded
        // so tokens and personal data never reach the logs.
        services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod |
                HttpLoggingFields.RequestPath |
                HttpLoggingFields.ResponseStatusCode |
                HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });

    private static void AddApiOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(ApiRoutes.DocumentName, options =>
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Enterprise Asset Management API";
                document.Info.Version = ApiRoutes.DocumentName;
                document.Info.Description =
                    "Backend API for the Enterprise Asset & Maintenance Management Platform.";
                return Task.CompletedTask;
            }));

    private static void AddApiHealthChecks(this IServiceCollection services) =>
        // Liveness only: answers "is the process running and serving requests?".
        // Dependency (readiness) checks are added with SQL Server, Redis and RabbitMQ.
        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("The API process is running."),
                tags: [HealthCheckTags.Live]);

    private static void AddApiCors(this IServiceCollection services)
    {
        services.AddOptions<CorsSettings>()
            .BindConfiguration(CorsSettings.SectionName)
            .Validate(
                settings => settings.AllowedOrigins.All(CorsSettings.IsValidOrigin),
                $"'{CorsSettings.SectionName}:AllowedOrigins' must contain only absolute http(s) origins " +
                "without wildcards, paths or trailing slashes (for example 'https://app.example.com').")
            .ValidateOnStart();

        services.AddCors();
        services.AddSingleton<IConfigureOptions<CorsOptions>, ConfigureCorsOptions>();
    }
}
