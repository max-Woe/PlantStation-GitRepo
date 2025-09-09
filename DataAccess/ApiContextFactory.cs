using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using DataAccess; 

public class ApiContextFactory : IDesignTimeDbContextFactory<ApiContext>
{
    public ApiContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApiContext>();
        // Gib hier deinen Connection String ein
        optionsBuilder.UseNpgsql("Host=192.168.178.75;Database=plantstationdatabase;Username=maxpowa;Password=Marianne1967");

        return new ApiContext(optionsBuilder.Options);
    }
}