using System.Collections.Generic;
using Infrastructure.Persistence.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Api;

public sealed class OpenApiOptions
{
    internal const string SectionName = nameof(OpenApiOptions);
    public string? Title { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string> Versions { get; init; } = ["v1"];
    public bool EnableBearerSecurity { get; init; } = true;
    public string BearerSchemeName { get; init; } = "Bearer";
    public string BearerFormat { get; init; } = "JWT";
}