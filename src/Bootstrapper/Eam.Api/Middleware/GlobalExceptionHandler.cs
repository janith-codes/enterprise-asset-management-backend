using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Eam.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into RFC 9457 ProblemDetails responses.
/// </summary>
/// <remarks>
/// Stack traces are never returned. Exception type and message are added only in Development.
/// </remarks>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>Non-standard status (nginx convention) recorded when the client aborts the request.</summary>
    public const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var request = httpContext.Request;

        // The client went away: nothing can be sent, and this is not a server fault.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            LogRequestAborted(logger, request.Method, request.Path);
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var (statusCode, title) = exception switch
        {
            BadHttpRequestException badRequest => (badRequest.StatusCode, "The request is invalid."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(logger, exception, request.Method, request.Path);
        }
        else
        {
            LogBadRequest(logger, exception, request.Method, request.Path, statusCode);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
        };

        if (environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["exception"] = exception.GetType().FullName;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Unhandled exception while processing {Method} {Path}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string method, PathString path);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Bad request {Method} {Path} returned {StatusCode}")]
    private static partial void LogBadRequest(ILogger logger, Exception exception, string method, PathString path, int statusCode);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Request {Method} {Path} was aborted by the client")]
    private static partial void LogRequestAborted(ILogger logger, string method, PathString path);
}
