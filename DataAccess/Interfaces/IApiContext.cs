using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Interfaces
{
    public interface IApiContext
    {
        /// <summary>
        /// Gets or sets the Measurements entity.
        /// </summary>
        DbSet<Measurement> Measurements { get; set; }

        /// <summary>
        /// Gets or sets the Sensor entity.
        /// </summary>
        DbSet<Sensor> Sensors { get; set; }

        /// <summary>
        /// Gets or sets the Station entity.
        /// </summary>
        DbSet<Station> Stations { get; set; }

        /// <summary>
        /// Gets or sets the AppExceptions entity.
        /// </summary>
        DbSet<AppException> AppExceptions { get; set; }

        /// <summary>
        /// Gets or sets the Plant entity.
        /// </summary>
        DbSet<Plant> Plants { get; set; }

        /// <summary>
        /// Gets or sets the PlantCharacteristic entity.
        /// </summary>
        DbSet<PlantCharacteristic> PlantCharacteristics { get; set; }

        /// <summary>
        /// Gets or sets the PlantName entity.
        /// </summary>
        DbSet<PlantName> PlantNames { get; set; }

        /// <summary>
        /// Gets or sets the PlantTyope entity.
        /// </summary>
        DbSet<PlantType> PlantTypes { get; set; }

        /// <summary>
        /// Gets or sets the TypeCharacteristic entity.
        /// </summary>
        DbSet<TypeCharacteristic> TypeCharacteristics { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
