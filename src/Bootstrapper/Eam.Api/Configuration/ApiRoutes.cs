namespace Eam.Api.Configuration;

/// <summary>
/// Route conventions shared by the host.
/// </summary>
internal static class ApiRoutes
{
    /// <summary>Version 1 of the public API. Every module is mapped under this prefix.</summary>
    public const string Prefix = "/api/v1";

    /// <summary>OpenAPI document name, served at <c>/openapi/v1.json</c>.</summary>
    public const string DocumentName = "v1";

    /// <summary>Liveness probe endpoint.</summary>
    public const string Liveness = "/health/live";
}
