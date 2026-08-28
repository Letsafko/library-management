using Application;
using Application.Features.Books;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Primitives;

namespace Infrastructure;

public static class DependencyInjectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddPipelineBehaviors();
        services.ConfigureOptions<DatabaseOptionsSetup>();
        services.AddDateTimeProvider();
        services.AddDatabase();
        services.AddRepositories();
    }
    private static void AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: databaseOptions.MaxRetryCount);
                npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
            });

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            options.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseLoggerFactory(loggerFactory);
            options.UseCamelCaseNamingConvention();
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
    }

    private static void AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    }
}