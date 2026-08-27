using System;
using System.Net.Http;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests.Infrastructure;

public sealed class ApplicationFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("library")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    internal CustomWebApplicationFactory Factory { get; private set; } = null!;
    internal ApplicationDbContext DbContext { get; private set; } = null!;
    internal HttpClient HttpClient { get; private set; } = null!;
    private IServiceScope? _scope;
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        var connectionString = _postgreSqlContainer.GetConnectionString();
        Factory = new CustomWebApplicationFactory(connectionString);
        HttpClient = Factory.CreateClient();
        
        _scope = Factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        await Factory.DisposeAsync();
        await DbContext.DisposeAsync();
        await _postgreSqlContainer.StopAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsync();
    }
}
