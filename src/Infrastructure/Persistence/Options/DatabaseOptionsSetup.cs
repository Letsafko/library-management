using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Options;

public sealed class DatabaseOptionsSetup(IConfiguration configuration) : IConfigureOptions<DatabaseOptions>
{
	private const string ConfigurationSectionName = nameof(DatabaseOptions);

	public void Configure(DatabaseOptions options)
	{
		configuration.GetSection(ConfigurationSectionName).Bind(options);
		var connectionString = configuration.GetConnectionString("database");
		options.ConnectionString = connectionString!;
	}
}