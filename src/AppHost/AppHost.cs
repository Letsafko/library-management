using AppHost;
using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres(PostgresConstants.ResourceName)
    .WithDataVolume()
    .WithPgWeb()
    .WithEnvironment("POSTGRES_INITDB_ARGS", PostgresConstants.InitDbArgs);

var database = postgres.AddDatabase(PostgresConstants.DatabaseResourceName, PostgresConstants.DatabaseName);

builder
    .AddProject<Api>(ServiceConstants.ApiName)
    .WithReference(database, connectionName: PostgresConstants.DatabaseResourceName)
    .WithReplicas(ServiceConstants.DefaultReplicas)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);