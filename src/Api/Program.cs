using Api;
using Api.Extensions;
using Application.Features.Books.Create;
using Asp.Versioning;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddCustomOpenApi(builder.Configuration);

builder.Services.AddInfrastructure();

builder.Services.AddOpenApi();

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<CreateBookCommandValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

var versionedGroup = app.GetVersionedGroupBuilder();
app.MapEndpoints(routeGroupBuilder: versionedGroup);

await app.RunAsync().ConfigureAwait(false);