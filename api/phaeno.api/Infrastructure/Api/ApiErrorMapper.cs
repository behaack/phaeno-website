using phaeno.api.Common.Exceptions;

namespace phaeno.api.Infrastructure.Api
{
    public static class ApiErrorMapper
    {
        public static (int StatusCode, ApiError Error) Map(Exception exception)
        {
            // DOMAIN / VALIDATION (400)
            if (exception is DomainException domain)
            {
                return (
                    StatusCodes.Status400BadRequest,
                    new ApiError(
                        type: "invalid_request",
                        code: domain.ErrorCode,
                        message: domain.Message
                    )
                );
            }

            // FALLBACK (500)
            return (
                StatusCodes.Status500InternalServerError,
                new ApiError(
                    type: "api_error",
                    code: "internal_error",
                    message: "An unexpected error occurred."
                )
            );
        }
    }
}
