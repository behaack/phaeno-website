using Microsoft.AspNetCore.Diagnostics;
using phaeno.api.Infrastructure.Api;
using System.Text.Json;

namespace phaeno.api.Middleware;

/// <summary>
/// API-only exception middleware that converts exceptions into a consistent ApiResponse.Fail envelope.
/// - Lets model-binding / malformed JSON continue to be handled by ASP.NET Core (400s)
/// - Avoids logging expected 4xx as "unhandled"
/// - Preserves request abort behavior
/// </summary>
public sealed class ApiExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        // Let ASP.NET Core handle request parsing/model binding failures (400)
        catch (Exception ex) when (ex is JsonException || ex is BadHttpRequestException)
        {
            throw;
        }
        // If the client disconnected, don't try to write a response
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Optional: log at Debug
            _logger.LogDebug("Request aborted by client.");
        }
        catch (Exception ex)
        {
            await WriteApiErrorAsync(context, ex);
        }
    }

    private async Task WriteApiErrorAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(exception, "Response already started; cannot write API error envelope.");
            return;
        }

        // Map exception -> (status, ApiError)
        var (statusCode, apiError) = ApiErrorMapper.Map(exception);

        // Log: only 5xx as errors; 4xx as info (expected)
        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception.");
        else
            _logger.LogInformation("Handled exception mapped to {StatusCode}: {Code}", statusCode, apiError.code);

        // Clear any partially-written headers/body
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        // Helpful: ensure caches don't store error responses
        context.Response.Headers.CacheControl = "no-store";

        // Preserve TraceIdentifier as requestId
        var meta = ApiMetaFactory.Create(context);
        var envelope = ApiResponse<object>.Fail(apiError, meta);

        // Write response
        await context.Response.WriteAsync(JsonSerializer.Serialize(envelope, JsonOptions));

        // Optional: mark exception as handled so built-in handlers don't double-handle
        context.Features.Get<IExceptionHandlerFeature>();
    }
}
