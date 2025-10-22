using DataAccess; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.IO;

/// <summary>
/// A factory class for creating an instance of the <see cref="ApiContext"/> DbContext at design time.
/// </summary>
/// <remarks>
/// This factory is essential for command-line tools like Entity Framework Core Migrations,
/// which need to instantiate the context without the application's runtime dependency injection container.
/// It explicitly loads configuration settings, including User Secrets, to retrieve the database connection string.
/// </remarks>
public class ApiContextFactory : IDesignTimeDbContextFactory<ApiContext>
{
    /// <summary>
    /// Creates a new instance of the <see cref="ApiContext"/>.
    /// </summary>
    /// <param name="args">Command line arguments (typically ignored in this context).</param>
    /// <returns>A configured instance of <see cref="ApiContext"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the 'DefaultConnection' connection string cannot be found in the configuration.</exception>
    public ApiContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<ApiContextFactory>()
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApiContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApiContext(optionsBuilder.Options);
    }
}