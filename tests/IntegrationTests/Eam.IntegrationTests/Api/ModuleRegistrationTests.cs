using Eam.Modules.Assets.Infrastructure;
using Eam.Modules.Assets.Presentation;
using Eam.Modules.Identity.Infrastructure;
using Eam.Modules.Identity.Presentation;
using Eam.Modules.Maintenance.Infrastructure;
using Eam.Modules.Maintenance.Presentation;
using Eam.Modules.Organization.Infrastructure;
using Eam.Modules.Organization.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Eam.IntegrationTests.Api;

public sealed class ModuleRegistrationTests
{
    public static TheoryData<string> Modules => ["Organization", "Identity", "Assets", "Maintenance"];

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_service_registration_executes_and_returns_same_collection(string module)
    {
        var services = new ServiceCollection();

        var result = module switch
        {
            "Organization" => services.AddOrganizationModule(),
            "Identity" => services.AddIdentityModule(),
            "Assets" => services.AddAssetsModule(),
            "Maintenance" => services.AddMaintenanceModule(),
            _ => throw new ArgumentOutOfRangeException(nameof(module)),
        };

        Assert.Same(services, result);
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public async Task Module_endpoint_registration_executes_and_returns_same_builder(string module)
    {
        await using var app = WebApplication.CreateSlimBuilder().Build();
        IEndpointRouteBuilder api = app.MapGroup("/api/v1");

        var result = module switch
        {
            "Organization" => api.MapOrganizationEndpoints(),
            "Identity" => api.MapIdentityEndpoints(),
            "Assets" => api.MapAssetEndpoints(),
            "Maintenance" => api.MapMaintenanceEndpoints(),
            _ => throw new ArgumentOutOfRangeException(nameof(module)),
        };

        Assert.Same(api, result);
    }

    [Fact]
    public void Registration_methods_reject_null_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddAssetsModule());
        Assert.Throws<ArgumentNullException>(() => ((IEndpointRouteBuilder)null!).MapAssetEndpoints());
    }
}
