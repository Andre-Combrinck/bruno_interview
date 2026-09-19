using System.Text.Json;

namespace Bruno.Api.Middleware;

internal static class ApiJsonSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
