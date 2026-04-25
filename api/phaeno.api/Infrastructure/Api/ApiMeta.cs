
namespace phaeno.api.Infrastructure.Api;

public sealed record ApiMeta(
    string requestId,
    DateTimeOffset timestampUtc
);
