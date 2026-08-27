using System.Collections.Generic;
using System.Linq;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Primitives;

namespace IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("DatabaseOptions:ConnectionString", connectionString)
            ]);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(new FakeDateTimeProvider());
            
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        });
    }
}