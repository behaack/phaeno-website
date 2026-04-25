namespace phaeno.api.Infrastructure.Api;

public sealed record ApiError(
    string type,      // auth_error, validation_error, rate_limit_error
    string code,      // invalid_credentials, two_factor_required
    string message,   // Human readable
    object? details = null,    
    string? param = null
);
