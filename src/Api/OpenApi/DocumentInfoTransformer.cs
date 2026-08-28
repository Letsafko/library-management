using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

internal sealed class DocumentInfoTransformer(OpenApiOptions options, string version)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = options.Title,
            Description = options.Description,
            Version = version
        };

        const string versionPlaceholder = "v{version}";
        var concreteVersion = version.Length > 0 && (version[0] == 'v' || version[0] == 'V')
            ? version
            : $"v{version}";

        var transformedPaths = new OpenApiPaths();
        foreach (var path in document.Paths)
        {
            var pathKey = path.Key.Replace(versionPlaceholder, concreteVersion, StringComparison.OrdinalIgnoreCase);
            transformedPaths.Add(pathKey, path.Value);
        }

        document.Paths = transformedPaths;

        return Task.CompletedTask;
    }
}
