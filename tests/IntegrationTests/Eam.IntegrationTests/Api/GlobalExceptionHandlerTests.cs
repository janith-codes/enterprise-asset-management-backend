using System.Text.Json;
using Eam.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;

namespace Eam.IntegrationTests.Api;

public sealed class GlobalExceptionHandlerTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Production_500_response_hides_exception_details()
    {
        var (handled, context, body) = await HandleAsync(
            Environments.Production,
            new InvalidOperationException("secret connection string"));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(500, body.GetProperty("status").GetInt32());
        Assert.False(body.TryGetProperty("detail", out _));
        Assert.False(body.TryGetProperty("exception", out _));
        Assert.DoesNotContain("secret", body.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Development_500_response_includes_message_but_no_stack_trace()
    {
        var (_, _, body) = await HandleAsync(
            Environments.Development,
            ThrownException());

        Assert.Equal("boom", body.GetProperty("detail").GetString());
        Assert.Equal(typeof(InvalidOperationException).FullName, body.GetProperty("exception").GetString());
        Assert.DoesNotContain(" at ", body.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Bad_request_exception_keeps_its_status_code()
    {
        var (_, context, body) = await HandleAsync(
            Environments.Production,
            new BadHttpRequestException("bad", StatusCodes.Status400BadRequest));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(400, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Client_aborted_request_returns_499_without_body()
    {
        var (handled, context, _) = await HandleAsync(
            Environments.Production,
            new OperationCanceledException(),
            clientAborted: true);

        Assert.True(handled);
        Assert.Equal(GlobalExceptionHandler.ClientClosedRequest, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    private static InvalidOperationException ThrownException()
    {
        try
        {
            throw new InvalidOperationException("boom");
        }
        catch (InvalidOperationException exception)
        {
            return exception; // has a real stack trace
        }
    }

    private static async Task<(bool Handled, HttpContext Context, JsonElement Body)> HandleAsync(
        string environmentName,
        Exception exception,
        bool clientAborted = false)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = services,
            RequestAborted = clientAborted ? new CancellationToken(canceled: true) : CancellationToken.None,
        };
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/test";
        context.Response.Body = new MemoryStream();

        var handler = new GlobalExceptionHandler(
            services.GetRequiredService<IProblemDetailsService>(),
            new HostingEnvironment { EnvironmentName = environmentName },
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, exception, Ct);

        context.Response.Body.Position = 0;
        var body = context.Response.Body.Length == 0
            ? default
            : await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body, cancellationToken: Ct);

        return (handled, context, body);
    }
}
