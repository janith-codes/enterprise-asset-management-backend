using Eam.Api.Extensions;

// Composition root: the only place that knows about every module.
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiHost()
    .AddModules();

var app = builder.Build();

app.UseApiPipeline();
app.MapApiEndpoints();

await app.RunAsync();
