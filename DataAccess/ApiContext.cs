using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;

namespace DataAccess;

/// <summary>
/// Represents an Database Context for Database access.
/// </summary>
public class ApiContext : DbContext, IApiContext
{
    /// <summary>
    /// Gets or sets the Measurements entity.
    /// </summary>
    public DbSet<Measurement> Measurements { get; set; }

    /// <summary>
    /// Gets or sets the Sensor entity.
    /// </summary>
    public DbSet<Sensor> Sensors { get; set; }

    /// <summary>
    /// Gets or sets the Station entity.
    /// </summary>
    public DbSet<Station> Stations { get; set; }

    /// <summary>
    /// Gets or sets the AppExceptions entity.
    /// </summary>
    public DbSet<AppException> AppExceptions { get; set; }

    /// <summary>
    /// Gets or sets the Plant entity.
    /// </summary>
    public DbSet<Plant> Plants { get; set; }

    /// <summary>
    /// Gets or sets the PlantCharacteristic entity.
    /// </summary>
    public DbSet<PlantCharacteristic> PlantCharacteristics { get; set; }

    /// <summary>
    /// Gets or sets the PlantName entity.
    /// </summary>
    public DbSet<PlantName> PlantNames { get; set; }

    /// <summary>
    /// Gets or sets the PlantTyope entity.
    /// </summary>
    public DbSet<PlantType> PlantTypes { get; set; }

    /// <summary>
    /// Gets or sets the TypeCharacteristic entity.
    /// </summary>
    public DbSet<TypeCharacteristic> TypeCharacteristics { get; set; }

    /// <summary>
    /// Constructor of the ApiContext.
    /// </summary>
    /// <value>
    /// Handles the options from the call to the base constructor.
    /// </value>
    /// <param name="options"></param>
    public ApiContext(DbContextOptions<ApiContext> options)
        : base(options)
    {

    }

    /// <summary>
    /// Configures the database model.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Measurement>()
            .HasIndex(m => m.RecordedAt);

        // Rufe die Basis-Methode auf, um sicherzustellen, dass Standardkonventionen angewendet werden
        base.OnModelCreating(modelBuilder);
    }
}