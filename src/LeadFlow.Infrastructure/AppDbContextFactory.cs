using LeadFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LeadFlow.Infrastructure;

/// <summary>
/// Used only by EF CLI tools (dotnet ef migrations add ...).
/// Never instantiated at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Walk up from Infrastructure/bin to find the API appsettings.json
        var basePath = Path.Combine(Directory.GetCurrentDirectory(),
            "..", "LeadFlow.API");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Postgres connection string not found.");

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(opts);
    }
}
