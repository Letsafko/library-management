using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions;

internal static class DatabaseMigrationExtensions
{
	internal static async Task ApplyMigrationsAsync(this WebApplication app)
	{
		await using var scope = app.Services.CreateAsyncScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await context.Database.MigrateAsync();
	}
}