using System.Text.Json.Serialization;
using SharedKernel.Primitives;

namespace IntegrationTests;

public sealed record ErrorTest
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("type")]
    public ErrorType Type { get; init; }
}