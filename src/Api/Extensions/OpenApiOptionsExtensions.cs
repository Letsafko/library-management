using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions;

internal static class OpenApiOptionsExtensions
{
    internal static void AddCustomOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new OpenApiOptions();
        configuration.GetSection(nameof(OpenApiOptions)).Bind(options);
        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = ApiVersion.Default;
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ReportApiVersions = true;
        });
    }
}