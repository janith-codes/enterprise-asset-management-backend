using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Eam.Api.Configuration;

/// <summary>
/// Builds the frontend CORS policy from validated <see cref="CorsSettings"/>.
/// </summary>
internal sealed class ConfigureCorsOptions(IOptions<CorsSettings> settings) : IConfigureOptions<CorsOptions>
{
    private static readonly TimeSpan PreflightMaxAge = TimeSpan.FromMinutes(10);

    public void Configure(CorsOptions options) =>
        options.AddPolicy(CorsSettings.PolicyName, policy => policy
            .WithOrigins(settings.Value.AllowedOrigins)
            .WithMethods(
                HttpMethods.Get,
                HttpMethods.Post,
                HttpMethods.Put,
                HttpMethods.Patch,
                HttpMethods.Delete)
            .AllowAnyHeader()
            .SetPreflightMaxAge(PreflightMaxAge));
}
