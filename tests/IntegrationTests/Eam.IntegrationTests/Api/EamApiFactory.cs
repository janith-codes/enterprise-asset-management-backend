using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Eam.IntegrationTests.Api;

/// <summary>
/// In-memory API host (TestServer) running in Development with a known frontend origin.
/// No external infrastructure is required.
/// </summary>
public sealed class EamApiFactory : WebApplicationFactory<Program>
{
    public const string AllowedOrigin = "https://frontend.test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = AllowedOrigin,
            }));
    }
}
