using Api.Extensions;
using Api.OpenApi;
using Application.Features.Books.Create;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddCustomOpenApi(builder.Configuration);

builder.Services.AddInfrastructure();

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<CreateBookCommandValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCustomOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

var versionedGroup = app.GetVersionedGroupBuilder();
app.MapEndpoints(routeGroupBuilder: versionedGroup);

await app.RunAsync();