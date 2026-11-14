namespace Eam.Api.Configuration;

/// <summary>
/// Tags used to select which health checks each probe endpoint runs.
/// </summary>
internal static class HealthCheckTags
{
    /// <summary>Checks that only verify the process itself is alive.</summary>
    public const string Live = "live";
}
