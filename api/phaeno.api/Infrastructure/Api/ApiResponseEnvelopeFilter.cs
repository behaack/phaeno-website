using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace phaeno.api.Infrastructure.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipApiEnvelopeAttribute : Attribute { }

public sealed class ApiResponseEnvelopeFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Opt-out for endpoints like file downloads/streams
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipApiEnvelopeAttribute>().Any())
        {
            await next();
            return;
        }

        // Skip non-JSON / framework results
        if (context.Result is FileResult
            or ChallengeResult
            or ForbidResult
            or SignInResult
            or SignOutResult
            or RedirectResult
            or RedirectToActionResult
            or RedirectToRouteResult)
        {
            await next();
            return;
        }

        // IMPORTANT: don't re-shape StatusCode results (404/401/204/etc)
        // Those should be produced by your exception pipeline or explicit envelope returns.
        if (context.Result is StatusCodeResult)
        {
            await next();
            return;
        }

        // Already wrapped? leave it alone
        if (context.Result is ObjectResult { Value: { } v } &&
            v.GetType().IsGenericType &&
            v.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            await next();
            return;
        }

        // Wrap ObjectResult (Ok(dto), Created(dto), etc.)
        if (context.Result is ObjectResult obj)
        {
            var meta = ApiMetaFactory.Create(context.HttpContext);
            var wrapped = ApiResponse<object?>.Ok(obj.Value, meta);

            context.Result = new ObjectResult(wrapped)
            {
                StatusCode = obj.StatusCode ?? StatusCodes.Status200OK
            };

            await next();
            return;
        }

        // Wrap plain OkResult (rare)
        if (context.Result is OkResult)
        {
            var meta = ApiMetaFactory.Create(context.HttpContext);
            context.Result = new ObjectResult(ApiResponse<object?>.Ok(null, meta))
            {
                StatusCode = StatusCodes.Status200OK
            };

            await next();
            return;
        }

        await next();
    }
}
