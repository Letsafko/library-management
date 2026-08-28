using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace Api.OpenApi;

internal static class OpenApiOptionsExtensions
{
    internal static void AddCustomOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new OpenApiOptions();
        configuration.GetSection(nameof(OpenApiOptions)).Bind(options);
        services.AddOptions<OpenApiOptions>().BindConfiguration(OpenApiOptions.SectionName);
        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = ApiVersion.Default;
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ReportApiVersions = true;
        });

        foreach (var version in options.Versions)
        {
            services.AddOpenApi(version, openApiOptions =>
            {
                openApiOptions.AddDocumentTransformer(new DocumentInfoTransformer(options, version));
            });
        }
    }
    
    public static void UseCustomOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapOpenApi();
        var options = app.Services.GetRequiredService<IOptions<OpenApiOptions>>().Value;
        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions
                .WithTitle(options.Title ?? string.Empty)
                .AddDocuments(options.Versions);
        });
    }
}