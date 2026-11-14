namespace Eam.Api.Configuration;

/// <summary>
/// CORS settings bound from the <c>Cors</c> configuration section.
/// </summary>
/// <remarks>
/// Set per environment. For example, in production through environment variables:
/// <c>Cors__AllowedOrigins__0=https://app.example.com</c>.
/// An empty list means no cross-origin requests are allowed.
/// </remarks>
internal sealed class CorsSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Cors";

    /// <summary>Name of the CORS policy applied to the API.</summary>
    public const string PolicyName = "Frontend";

    /// <summary>Exact origins of the React frontend, for example <c>https://app.example.com</c>.</summary>
    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="origin"/> is a concrete http(s) origin:
    /// scheme, host and optional port only. Wildcards, paths, query strings and trailing slashes are rejected.
    /// </summary>
    public static bool IsValidOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin) || origin.Contains('*', StringComparison.Ordinal) || origin.EndsWith('/'))
        {
            return false;
        }

        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && uri.AbsolutePath == "/"
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment)
            && string.IsNullOrEmpty(uri.UserInfo);
    }
}
