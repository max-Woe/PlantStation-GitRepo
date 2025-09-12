using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

/// <summary>
/// Represents a sensor entity.
/// </summary>
public class Sensor
{
    /// <summary>
    /// Gets or sets the unique identifier id.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the sensor type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the sensor unit.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets the water level measurement.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the station id.
    /// </summary>
    public int StationId { get; set; }

    /// <summary>
    /// Gets or sets the station.
    /// </summary>
    public Station? Station { get; set; }

    /// <summary>
    /// Gets or sets the time when the sensor first was captured in the database.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the time when the sensor was updated last.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Updates the sensor entry including type, unit, deviceId, stationId station and updatedAt.
    /// </summary>
    /// <param name="sensor">
    /// An sensor object with all new values for the sensor update.
    /// </param>
    public void Update(Sensor sensor)
    {
        Type = sensor.Type;
        Unit = sensor.Unit;
        DeviceId = sensor.DeviceId;
        StationId = sensor.StationId;
        Station = sensor.Station;
        UpdatedAt = sensor.UpdatedAt;
    }
}